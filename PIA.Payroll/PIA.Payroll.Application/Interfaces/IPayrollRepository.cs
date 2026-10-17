using PIA.Payroll.Application.DTOs;
using PIA.Payroll.Domain.Entities;

namespace PIA.Payroll.Application.Interfaces;

public interface IPayrollRepository
{
    // Periods
    Task<IReadOnlyList<PayrollPeriod>> GetAllPeriodsAsync(CancellationToken cancellationToken = default);
    Task<PayrollPeriod?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PayrollPeriod?> GetPeriodByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<PayrollPeriod> AddPeriodAsync(PayrollPeriod period, CancellationToken cancellationToken = default);
    Task UpdatePeriodAsync(PayrollPeriod period, CancellationToken cancellationToken = default);

    // Salary Structures
    Task<IReadOnlyList<SalaryStructureDto>> GetAllSalaryStructuresAsync(CancellationToken cancellationToken = default);
    Task<SalaryStructure?> GetSalaryStructureByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<SalaryStructure> SaveSalaryStructureAsync(SalaryStructure salaryStructure, CancellationToken cancellationToken = default);

    // Payslips
    Task<IReadOnlyList<PayslipDto>> GetPayslipsByPeriodIdAsync(int periodId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayslipDto>> GetPayslipsByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<PayslipDto?> GetPayslipDetailsAsync(int payslipId, CancellationToken cancellationToken = default);
    Task<Payslip?> GetPayslipEntityAsync(int periodId, int employeeId, CancellationToken cancellationToken = default);
    Task AddPayslipsAsync(IEnumerable<Payslip> payslips, CancellationToken cancellationToken = default);
    Task RemovePayslipsForPeriodAsync(int periodId, CancellationToken cancellationToken = default);
    Task UpdatePayslipAsync(Payslip payslip, CancellationToken cancellationToken = default);

    // Dashboard
    Task<PayrollDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
