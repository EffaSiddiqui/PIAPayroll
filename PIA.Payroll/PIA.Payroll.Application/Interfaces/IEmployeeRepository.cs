using PIA.Payroll.Application.DTOs;
using PIA.Payroll.Domain.Entities;

namespace PIA.Payroll.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<EmployeeListItemDto>> GetEmployeesAsync(EmployeeFilterDto filter, CancellationToken cancellationToken = default);
    Task<EmployeeDetailsDto?> GetEmployeeDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string employeeCode, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCnicAsync(string cnic, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LookupItemDto>> GetDepartmentLookupsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LookupItemDto>> GetDesignationLookupsAsync(CancellationToken cancellationToken = default);
}
