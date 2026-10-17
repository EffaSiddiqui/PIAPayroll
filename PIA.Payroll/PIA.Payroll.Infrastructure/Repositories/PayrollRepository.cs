using Microsoft.EntityFrameworkCore;
using PIA.Payroll.Application.DTOs;
using PIA.Payroll.Application.Interfaces;
using PIA.Payroll.Domain.Entities;
using PIA.Payroll.Infrastructure.Data;

namespace PIA.Payroll.Infrastructure.Repositories;

public class PayrollRepository : IPayrollRepository
{
    private readonly PayrollDbContext _context;

    public PayrollRepository(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PayrollPeriod>> GetAllPeriodsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PayrollPeriods
            .AsNoTracking()
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<PayrollPeriod?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PayrollPeriod?> GetPeriodByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.PayrollPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Year == year && p.Month == month, cancellationToken);
    }

    public async Task<PayrollPeriod> AddPeriodAsync(PayrollPeriod period, CancellationToken cancellationToken = default)
    {
        await _context.PayrollPeriods.AddAsync(period, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return period;
    }

    public async Task UpdatePeriodAsync(PayrollPeriod period, CancellationToken cancellationToken = default)
    {
        _context.PayrollPeriods.Update(period);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SalaryStructureDto>> GetAllSalaryStructuresAsync(CancellationToken cancellationToken = default)
    {
        var query = from emp in _context.Employees.AsNoTracking()
                    join s in _context.SalaryStructures.AsNoTracking() on emp.Id equals s.EmployeeId into structGroup
                    from s in structGroup.DefaultIfEmpty()
                    join dept in _context.Departments.AsNoTracking() on emp.DepartmentId equals dept.Id into deptGroup
                    from dept in deptGroup.DefaultIfEmpty()
                    join desig in _context.Designations.AsNoTracking() on emp.DesignationId equals desig.Id into desigGroup
                    from desig in desigGroup.DefaultIfEmpty()
                    orderby emp.EmployeeCode
                    select new SalaryStructureDto
                    {
                        Id = s != null ? s.Id : 0,
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        EmployeeName = emp.FirstName + " " + emp.LastName,
                        DepartmentName = dept != null ? dept.Name : null,
                        DesignationName = desig != null ? desig.Name : null,
                        BasicSalary = s != null ? s.BasicSalary : 0m,
                        HouseRentAllowance = s != null ? s.HouseRentAllowance : 0m,
                        MedicalAllowance = s != null ? s.MedicalAllowance : 0m,
                        SpecialAllowance = s != null ? s.SpecialAllowance : 0m,
                        IncomeTaxDeduction = s != null ? s.IncomeTaxDeduction : 0m,
                        ProvidentFundDeduction = s != null ? s.ProvidentFundDeduction : 0m,
                        OtherDeductions = s != null ? s.OtherDeductions : 0m,
                        EffectiveDate = s != null ? s.EffectiveDate : emp.JoiningDate,
                        IsActive = s != null ? s.IsActive : (emp.EmploymentStatus == "Active")
                    };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<SalaryStructure?> GetSalaryStructureByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.SalaryStructures
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId, cancellationToken);
    }

    public async Task<SalaryStructure> SaveSalaryStructureAsync(SalaryStructure salaryStructure, CancellationToken cancellationToken = default)
    {
        if (salaryStructure.Id == 0)
        {
            await _context.SalaryStructures.AddAsync(salaryStructure, cancellationToken);
        }
        else
        {
            _context.SalaryStructures.Update(salaryStructure);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return salaryStructure;
    }

    public async Task<IReadOnlyList<PayslipDto>> GetPayslipsByPeriodIdAsync(int periodId, CancellationToken cancellationToken = default)
    {
        var query = from p in _context.Payslips.AsNoTracking()
                    where p.PayrollPeriodId == periodId
                    join period in _context.PayrollPeriods.AsNoTracking() on p.PayrollPeriodId equals period.Id
                    join emp in _context.Employees.AsNoTracking() on p.EmployeeId equals emp.Id
                    join dept in _context.Departments.AsNoTracking() on emp.DepartmentId equals dept.Id into deptGroup
                    from dept in deptGroup.DefaultIfEmpty()
                    join desig in _context.Designations.AsNoTracking() on emp.DesignationId equals desig.Id into desigGroup
                    from desig in desigGroup.DefaultIfEmpty()
                    orderby emp.EmployeeCode
                    select new PayslipDto
                    {
                        Id = p.Id,
                        PayrollPeriodId = p.PayrollPeriodId,
                        PeriodName = period.PeriodName,
                        Year = period.Year,
                        Month = period.Month,
                        PeriodStatus = period.Status,
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        EmployeeName = emp.FirstName + " " + emp.LastName,
                        FatherName = emp.FatherName,
                        CNIC = emp.CNIC,
                        OfficialEmail = emp.OfficialEmail,
                        DepartmentName = dept != null ? dept.Name : null,
                        DesignationName = desig != null ? desig.Name : null,
                        BasicSalary = p.BasicSalary,
                        HouseRentAllowance = p.HouseRentAllowance,
                        MedicalAllowance = p.MedicalAllowance,
                        SpecialAllowance = p.SpecialAllowance,
                        TotalAllowances = p.TotalAllowances,
                        GrossSalary = p.GrossSalary,
                        IncomeTaxDeduction = p.IncomeTaxDeduction,
                        ProvidentFundDeduction = p.ProvidentFundDeduction,
                        OtherDeductions = p.OtherDeductions,
                        TotalDeductions = p.TotalDeductions,
                        NetSalary = p.NetSalary,
                        PaymentStatus = p.PaymentStatus,
                        PaymentDate = p.PaymentDate,
                        GeneratedDate = p.GeneratedDate,
                        Remarks = p.Remarks
                    };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PayslipDto>> GetPayslipsByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var query = from p in _context.Payslips.AsNoTracking()
                    where p.EmployeeId == employeeId
                    join period in _context.PayrollPeriods.AsNoTracking() on p.PayrollPeriodId equals period.Id
                    join emp in _context.Employees.AsNoTracking() on p.EmployeeId equals emp.Id
                    join dept in _context.Departments.AsNoTracking() on emp.DepartmentId equals dept.Id into deptGroup
                    from dept in deptGroup.DefaultIfEmpty()
                    join desig in _context.Designations.AsNoTracking() on emp.DesignationId equals desig.Id into desigGroup
                    from desig in desigGroup.DefaultIfEmpty()
                    orderby period.Year descending, period.Month descending
                    select new PayslipDto
                    {
                        Id = p.Id,
                        PayrollPeriodId = p.PayrollPeriodId,
                        PeriodName = period.PeriodName,
                        Year = period.Year,
                        Month = period.Month,
                        PeriodStatus = period.Status,
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        EmployeeName = emp.FirstName + " " + emp.LastName,
                        FatherName = emp.FatherName,
                        CNIC = emp.CNIC,
                        OfficialEmail = emp.OfficialEmail,
                        DepartmentName = dept != null ? dept.Name : null,
                        DesignationName = desig != null ? desig.Name : null,
                        BasicSalary = p.BasicSalary,
                        HouseRentAllowance = p.HouseRentAllowance,
                        MedicalAllowance = p.MedicalAllowance,
                        SpecialAllowance = p.SpecialAllowance,
                        TotalAllowances = p.TotalAllowances,
                        GrossSalary = p.GrossSalary,
                        IncomeTaxDeduction = p.IncomeTaxDeduction,
                        ProvidentFundDeduction = p.ProvidentFundDeduction,
                        OtherDeductions = p.OtherDeductions,
                        TotalDeductions = p.TotalDeductions,
                        NetSalary = p.NetSalary,
                        PaymentStatus = p.PaymentStatus,
                        PaymentDate = p.PaymentDate,
                        GeneratedDate = p.GeneratedDate,
                        Remarks = p.Remarks
                    };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PayslipDto?> GetPayslipDetailsAsync(int payslipId, CancellationToken cancellationToken = default)
    {
        var query = from p in _context.Payslips.AsNoTracking()
                    where p.Id == payslipId
                    join period in _context.PayrollPeriods.AsNoTracking() on p.PayrollPeriodId equals period.Id
                    join emp in _context.Employees.AsNoTracking() on p.EmployeeId equals emp.Id
                    join dept in _context.Departments.AsNoTracking() on emp.DepartmentId equals dept.Id into deptGroup
                    from dept in deptGroup.DefaultIfEmpty()
                    join desig in _context.Designations.AsNoTracking() on emp.DesignationId equals desig.Id into desigGroup
                    from desig in desigGroup.DefaultIfEmpty()
                    select new PayslipDto
                    {
                        Id = p.Id,
                        PayrollPeriodId = p.PayrollPeriodId,
                        PeriodName = period.PeriodName,
                        Year = period.Year,
                        Month = period.Month,
                        PeriodStatus = period.Status,
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        EmployeeName = emp.FirstName + " " + emp.LastName,
                        FatherName = emp.FatherName,
                        CNIC = emp.CNIC,
                        OfficialEmail = emp.OfficialEmail,
                        DepartmentName = dept != null ? dept.Name : null,
                        DesignationName = desig != null ? desig.Name : null,
                        BasicSalary = p.BasicSalary,
                        HouseRentAllowance = p.HouseRentAllowance,
                        MedicalAllowance = p.MedicalAllowance,
                        SpecialAllowance = p.SpecialAllowance,
                        TotalAllowances = p.TotalAllowances,
                        GrossSalary = p.GrossSalary,
                        IncomeTaxDeduction = p.IncomeTaxDeduction,
                        ProvidentFundDeduction = p.ProvidentFundDeduction,
                        OtherDeductions = p.OtherDeductions,
                        TotalDeductions = p.TotalDeductions,
                        NetSalary = p.NetSalary,
                        PaymentStatus = p.PaymentStatus,
                        PaymentDate = p.PaymentDate,
                        GeneratedDate = p.GeneratedDate,
                        Remarks = p.Remarks
                    };

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Payslip?> GetPayslipEntityAsync(int periodId, int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Payslips
            .FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.EmployeeId == employeeId, cancellationToken);
    }

    public async Task AddPayslipsAsync(IEnumerable<Payslip> payslips, CancellationToken cancellationToken = default)
    {
        await _context.Payslips.AddRangeAsync(payslips, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePayslipsForPeriodAsync(int periodId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Payslips
            .Where(p => p.PayrollPeriodId == periodId)
            .ToListAsync(cancellationToken);

        if (existing.Any())
        {
            _context.Payslips.RemoveRange(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdatePayslipAsync(Payslip payslip, CancellationToken cancellationToken = default)
    {
        _context.Payslips.Update(payslip);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PayrollDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var periods = await _context.PayrollPeriods.AsNoTracking().ToListAsync(cancellationToken);
        var activeEmployees = await _context.Employees.AsNoTracking().CountAsync(e => e.EmploymentStatus == "Active", cancellationToken);

        return new PayrollDashboardSummaryDto
        {
            TotalPeriods = periods.Count,
            ProcessedPeriodsCount = periods.Count(p => p.Status != "Draft"),
            TotalNetDisbursed = periods.Where(p => p.Status == "Paid").Sum(p => p.TotalNetSalary),
            ActiveEmployeesCount = activeEmployees
        };
    }
}
