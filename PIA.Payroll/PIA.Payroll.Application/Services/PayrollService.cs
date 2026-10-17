using System.Globalization;
using PIA.Payroll.Application.DTOs;
using PIA.Payroll.Application.Interfaces;
using PIA.Payroll.Domain.Entities;

namespace PIA.Payroll.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public PayrollService(
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository)
    {
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<IReadOnlyList<PayrollPeriodDto>> GetAllPeriodsAsync(CancellationToken cancellationToken = default)
    {
        var periods = await _payrollRepository.GetAllPeriodsAsync(cancellationToken);
        return periods.Select(MapToPeriodDto).ToList();
    }

    public async Task<PayrollPeriodDto?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var period = await _payrollRepository.GetPeriodByIdAsync(id, cancellationToken);
        return period == null ? null : MapToPeriodDto(period);
    }

    public async Task<(bool Success, string? ErrorMessage, int PeriodId)> CreatePeriodAsync(CreatePayrollPeriodDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.StartDate > dto.EndDate)
        {
            return (false, "Cycle start date must be before or equal to cycle end date.", 0);
        }

        // Prevent duplicate payroll periods for the same month and year
        var existing = await _payrollRepository.GetPeriodByYearMonthAsync(dto.Year, dto.Month, cancellationToken);
        if (existing != null)
        {
            return (false, $"A payroll period for {existing.PeriodName} already exists.", 0);
        }

        var periodName = new DateTime(dto.Year, dto.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        var period = new PayrollPeriod
        {
            Year = dto.Year,
            Month = dto.Month,
            PeriodName = periodName,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            Status = "Draft",
            Notes = dto.Notes?.Trim(),
            TotalEmployeesProcessed = 0,
            TotalGrossSalary = 0m,
            TotalDeductions = 0m,
            TotalNetSalary = 0m
        };

        var created = await _payrollRepository.AddPeriodAsync(period, cancellationToken);
        return (true, null, created.Id);
    }

    public async Task<(bool Success, string? ErrorMessage, ProcessPayrollResultDto? Result)> ProcessPeriodAsync(
        int periodId,
        string processedBy,
        CancellationToken cancellationToken = default)
    {
        var period = await _payrollRepository.GetPeriodByIdAsync(periodId, cancellationToken);
        if (period == null)
        {
            return (false, "Payroll period not found.", null);
        }

        if (string.Equals(period.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "This payroll cycle has already been marked as Paid and finalized. It cannot be reprocessed.", null);
        }

        // Load active employees only (do not process inactive employees)
        var activeEmployeesFilter = new EmployeeFilterDto { Status = "Active" };
        var activeEmployees = await _employeeRepository.GetEmployeesAsync(activeEmployeesFilter, cancellationToken);

        if (!activeEmployees.Any())
        {
            return (false, "No active employees found to process for this payroll cycle.", null);
        }

        // Remove existing payslips for this period to prevent duplicate processing
        await _payrollRepository.RemovePayslipsForPeriodAsync(period.Id, cancellationToken);

        var newPayslips = new List<Payslip>();

        foreach (var emp in activeEmployees)
        {
            // Retrieve salary structure or fallback to default entry if not yet defined
            var structure = await _payrollRepository.GetSalaryStructureByEmployeeIdAsync(emp.Id, cancellationToken);

            decimal basicSalary = structure?.BasicSalary ?? 60_000m;
            decimal houseRent = structure?.HouseRentAllowance ?? (basicSalary * 0.45m);
            decimal medical = structure?.MedicalAllowance ?? (basicSalary * 0.10m);
            decimal special = structure?.SpecialAllowance ?? 0m;
            decimal totalAllowances = houseRent + medical + special;
            decimal grossSalary = basicSalary + totalAllowances;

            decimal tax = structure?.IncomeTaxDeduction ?? (grossSalary > 100_000m ? (grossSalary - 100_000m) * 0.05m : 0m);
            decimal pf = structure?.ProvidentFundDeduction ?? (basicSalary * 0.08m);
            decimal other = structure?.OtherDeductions ?? 0m;
            decimal totalDeductions = tax + pf + other;
            decimal netSalary = grossSalary - totalDeductions;

            newPayslips.Add(new Payslip
            {
                PayrollPeriodId = period.Id,
                EmployeeId = emp.Id,
                BasicSalary = basicSalary,
                HouseRentAllowance = houseRent,
                MedicalAllowance = medical,
                SpecialAllowance = special,
                TotalAllowances = totalAllowances,
                GrossSalary = grossSalary,
                IncomeTaxDeduction = tax,
                ProvidentFundDeduction = pf,
                OtherDeductions = other,
                TotalDeductions = totalDeductions,
                NetSalary = netSalary,
                PaymentStatus = "Pending",
                GeneratedDate = DateTime.UtcNow
            });
        }

        await _payrollRepository.AddPayslipsAsync(newPayslips, cancellationToken);

        // Update period totals
        period.TotalEmployeesProcessed = newPayslips.Count;
        period.TotalGrossSalary = newPayslips.Sum(p => p.GrossSalary);
        period.TotalDeductions = newPayslips.Sum(p => p.TotalDeductions);
        period.TotalNetSalary = newPayslips.Sum(p => p.NetSalary);
        period.Status = "Processed";
        period.ProcessedDate = DateTime.UtcNow;
        period.ProcessedBy = processedBy;

        await _payrollRepository.UpdatePeriodAsync(period, cancellationToken);

        var result = new ProcessPayrollResultDto
        {
            Success = true,
            EmployeesProcessed = newPayslips.Count,
            TotalGross = period.TotalGrossSalary,
            TotalNet = period.TotalNetSalary
        };

        return (true, null, result);
    }

    public async Task<(bool Success, string? ErrorMessage)> MarkPeriodAsPaidAsync(int periodId, CancellationToken cancellationToken = default)
    {
        var period = await _payrollRepository.GetPeriodByIdAsync(periodId, cancellationToken);
        if (period == null)
        {
            return (false, "Payroll period not found.");
        }

        if (string.Equals(period.Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Draft payroll period must be processed before marking as Paid.");
        }

        period.Status = "Paid";
        await _payrollRepository.UpdatePeriodAsync(period, cancellationToken);

        var payslips = await _payrollRepository.GetPayslipsByPeriodIdAsync(periodId, cancellationToken);
        foreach (var slip in payslips)
        {
            var entity = await _payrollRepository.GetPayslipEntityAsync(slip.PayrollPeriodId, slip.EmployeeId, cancellationToken);
            if (entity != null)
            {
                entity.PaymentStatus = "Paid";
                entity.PaymentDate = DateTime.UtcNow;
                await _payrollRepository.UpdatePayslipAsync(entity, cancellationToken);
            }
        }

        return (true, null);
    }

    public async Task<IReadOnlyList<PayslipDto>> GetPayslipsForPeriodAsync(int periodId, CancellationToken cancellationToken = default)
    {
        return await _payrollRepository.GetPayslipsByPeriodIdAsync(periodId, cancellationToken);
    }

    public async Task<IReadOnlyList<PayslipDto>> GetEmployeePayslipsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _payrollRepository.GetPayslipsByEmployeeIdAsync(employeeId, cancellationToken);
    }

    public async Task<PayslipDto?> GetPayslipDetailsAsync(int payslipId, CancellationToken cancellationToken = default)
    {
        return await _payrollRepository.GetPayslipDetailsAsync(payslipId, cancellationToken);
    }

    public async Task<IReadOnlyList<SalaryStructureDto>> GetAllSalaryStructuresAsync(CancellationToken cancellationToken = default)
    {
        return await _payrollRepository.GetAllSalaryStructuresAsync(cancellationToken);
    }

    public async Task<SalaryStructureEditDto?> GetSalaryStructureForEditAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null) return null;

        var structure = await _payrollRepository.GetSalaryStructureByEmployeeIdAsync(employeeId, cancellationToken);

        if (structure == null)
        {
            return new SalaryStructureEditDto
            {
                Id = 0,
                EmployeeId = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                BasicSalary = 50_000m,
                HouseRentAllowance = 22_500m,
                MedicalAllowance = 5_000m,
                SpecialAllowance = 0m,
                IncomeTaxDeduction = 0m,
                ProvidentFundDeduction = 4_000m,
                OtherDeductions = 0m,
                EffectiveDate = DateTime.Today,
                IsActive = true
            };
        }

        return new SalaryStructureEditDto
        {
            Id = structure.Id,
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            BasicSalary = structure.BasicSalary,
            HouseRentAllowance = structure.HouseRentAllowance,
            MedicalAllowance = structure.MedicalAllowance,
            SpecialAllowance = structure.SpecialAllowance,
            IncomeTaxDeduction = structure.IncomeTaxDeduction,
            ProvidentFundDeduction = structure.ProvidentFundDeduction,
            OtherDeductions = structure.OtherDeductions,
            EffectiveDate = structure.EffectiveDate,
            IsActive = structure.IsActive
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> SaveSalaryStructureAsync(SalaryStructureEditDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return (false, "Employee not found.");
        }

        var existing = await _payrollRepository.GetSalaryStructureByEmployeeIdAsync(dto.EmployeeId, cancellationToken);

        if (existing == null)
        {
            var newStructure = new SalaryStructure
            {
                EmployeeId = dto.EmployeeId,
                BasicSalary = dto.BasicSalary,
                HouseRentAllowance = dto.HouseRentAllowance,
                MedicalAllowance = dto.MedicalAllowance,
                SpecialAllowance = dto.SpecialAllowance,
                IncomeTaxDeduction = dto.IncomeTaxDeduction,
                ProvidentFundDeduction = dto.ProvidentFundDeduction,
                OtherDeductions = dto.OtherDeductions,
                EffectiveDate = dto.EffectiveDate.Date,
                IsActive = dto.IsActive
            };
            await _payrollRepository.SaveSalaryStructureAsync(newStructure, cancellationToken);
        }
        else
        {
            existing.BasicSalary = dto.BasicSalary;
            existing.HouseRentAllowance = dto.HouseRentAllowance;
            existing.MedicalAllowance = dto.MedicalAllowance;
            existing.SpecialAllowance = dto.SpecialAllowance;
            existing.IncomeTaxDeduction = dto.IncomeTaxDeduction;
            existing.ProvidentFundDeduction = dto.ProvidentFundDeduction;
            existing.OtherDeductions = dto.OtherDeductions;
            existing.EffectiveDate = dto.EffectiveDate.Date;
            existing.IsActive = dto.IsActive;
            await _payrollRepository.SaveSalaryStructureAsync(existing, cancellationToken);
        }

        return (true, null);
    }

    public async Task<PayrollDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        return await _payrollRepository.GetDashboardSummaryAsync(cancellationToken);
    }

    private static PayrollPeriodDto MapToPeriodDto(PayrollPeriod p)
    {
        return new PayrollPeriodDto
        {
            Id = p.Id,
            Year = p.Year,
            Month = p.Month,
            PeriodName = p.PeriodName,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status,
            ProcessedDate = p.ProcessedDate,
            ProcessedBy = p.ProcessedBy,
            TotalEmployeesProcessed = p.TotalEmployeesProcessed,
            TotalGrossSalary = p.TotalGrossSalary,
            TotalDeductions = p.TotalDeductions,
            TotalNetSalary = p.TotalNetSalary,
            Notes = p.Notes
        };
    }
}
