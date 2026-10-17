using PIA.Payroll.Application.DTOs;

namespace PIA.Payroll.Application.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeListItemDto>> GetAllEmployeesAsync(EmployeeFilterDto filter, CancellationToken cancellationToken = default);
    Task<EmployeeDetailsDto?> GetEmployeeDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeEditDto?> GetEmployeeForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage, int EmployeeId)> CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> UpdateEmployeeAsync(EmployeeEditDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage)> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ErrorMessage, string NewStatus)> ToggleStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LookupItemDto>> GetDepartmentLookupsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LookupItemDto>> GetDesignationLookupsAsync(CancellationToken cancellationToken = default);
}
