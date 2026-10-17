using PIA.Payroll.Application.DTOs;

namespace PIA.Payroll.Application.Interfaces;

public interface IPayrollService
{
    // Periods
    Task<IReadOnlyList<PayrollPeriodDto>> GetAllPeriodsAsync(CancellationToken cancellationToken = default);
    Task<PayrollPeriodDto?> GetPeriodByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage, int PeriodId)> CreatePeriodAsync(CreatePayrollPeriodDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage, ProcessPayrollResultDto? Result)> ProcessPeriodAsync(int periodId, string processedBy, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> MarkPeriodAsPaidAsync(int periodId, CancellationToken cancellationToken = default);

    // Payslips
    Task<IReadOnlyList<PayslipDto>> GetPayslipsForPeriodAsync(int periodId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayslipDto>> GetEmployeePayslipsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<PayslipDto?> GetPayslipDetailsAsync(int payslipId, CancellationToken cancellationToken = default);

    // Salary Structures
    Task<IReadOnlyList<SalaryStructureDto>> GetAllSalaryStructuresAsync(CancellationToken cancellationToken = default);
    Task<SalaryStructureEditDto?> GetSalaryStructureForEditAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> SaveSalaryStructureAsync(SalaryStructureEditDto dto, CancellationToken cancellationToken = default);

    // Summary
    Task<PayrollDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
