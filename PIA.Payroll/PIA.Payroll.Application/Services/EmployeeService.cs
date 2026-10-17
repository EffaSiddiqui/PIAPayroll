using PIA.Payroll.Application.DTOs;
using PIA.Payroll.Application.Interfaces;
using PIA.Payroll.Domain.Entities;

namespace PIA.Payroll.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<EmployeeListItemDto>> GetAllEmployeesAsync(EmployeeFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await _repository.GetEmployeesAsync(filter, cancellationToken);
    }

    public async Task<EmployeeDetailsDto?> GetEmployeeDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetEmployeeDetailsAsync(id, cancellationToken);
    }

    public async Task<EmployeeEditDto?> GetEmployeeForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee == null) return null;

        return new EmployeeEditDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FatherName = employee.FatherName,
            CNIC = employee.CNIC,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender,
            PersonalEmail = employee.PersonalEmail,
            OfficialEmail = employee.OfficialEmail,
            PhoneNumber = employee.PhoneNumber,
            JoiningDate = employee.JoiningDate,
            EmploymentStatus = employee.EmploymentStatus,
            DepartmentId = employee.DepartmentId,
            DesignationId = employee.DesignationId,
            Extension = employee.Extension,
            CellPhoneNumber = employee.CellPhoneNumber,
            Location = employee.Location
        };
    }

    public async Task<(bool Success, string? ErrorMessage, int EmployeeId)> CreateEmployeeAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validation: Age check (min 18 years)
        var age = DateTime.Today.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        if (age < 18)
        {
            return (false, "Employee must be at least 18 years of age.", 0);
        }

        // 2. Uniqueness check for Employee Code
        if (await _repository.ExistsByCodeAsync(dto.EmployeeCode.Trim(), cancellationToken: cancellationToken))
        {
            return (false, $"Employee Code '{dto.EmployeeCode}' is already assigned to another employee.", 0);
        }

        // 3. Uniqueness check for CNIC
        if (await _repository.ExistsByCnicAsync(dto.CNIC.Trim(), cancellationToken: cancellationToken))
        {
            return (false, $"CNIC '{dto.CNIC}' is already registered with an existing employee.", 0);
        }

        // 4. Uniqueness check for Official Email
        if (await _repository.ExistsByEmailAsync(dto.OfficialEmail.Trim(), cancellationToken: cancellationToken))
        {
            return (false, $"Official Email '{dto.OfficialEmail}' is already assigned to another employee.", 0);
        }

        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode.Trim().ToUpperInvariant(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            FatherName = dto.FatherName.Trim(),
            CNIC = dto.CNIC.Trim(),
            DateOfBirth = dto.DateOfBirth.Date,
            Gender = dto.Gender,
            PersonalEmail = dto.PersonalEmail.Trim(),
            OfficialEmail = dto.OfficialEmail.Trim().ToLowerInvariant(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            JoiningDate = dto.JoiningDate.Date,
            EmploymentStatus = string.IsNullOrWhiteSpace(dto.EmploymentStatus) ? "Active" : dto.EmploymentStatus,
            DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId,
            Extension = dto.Extension?.Trim() ?? string.Empty,
            CellPhoneNumber = dto.CellPhoneNumber?.Trim() ?? string.Empty,
            Location = dto.Location?.Trim() ?? string.Empty
        };

        var created = await _repository.AddAsync(employee, cancellationToken);
        return (true, null, created.Id);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateEmployeeAsync(EmployeeEditDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (employee == null)
        {
            return (false, "Employee record not found.");
        }

        // Age check
        var age = DateTime.Today.Year - dto.DateOfBirth.Year;
        if (dto.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
        if (age < 18)
        {
            return (false, "Employee must be at least 18 years of age.");
        }

        // Uniqueness checks (excluding current employee Id)
        if (await _repository.ExistsByCodeAsync(dto.EmployeeCode.Trim(), dto.Id, cancellationToken))
        {
            return (false, $"Employee Code '{dto.EmployeeCode}' is already registered to another employee.");
        }

        if (await _repository.ExistsByCnicAsync(dto.CNIC.Trim(), dto.Id, cancellationToken))
        {
            return (false, $"CNIC '{dto.CNIC}' is already assigned to another employee.");
        }

        if (await _repository.ExistsByEmailAsync(dto.OfficialEmail.Trim(), dto.Id, cancellationToken))
        {
            return (false, $"Official Email '{dto.OfficialEmail}' is already registered to another employee.");
        }

        employee.EmployeeCode = dto.EmployeeCode.Trim().ToUpperInvariant();
        employee.FirstName = dto.FirstName.Trim();
        employee.LastName = dto.LastName.Trim();
        employee.FatherName = dto.FatherName.Trim();
        employee.CNIC = dto.CNIC.Trim();
        employee.DateOfBirth = dto.DateOfBirth.Date;
        employee.Gender = dto.Gender;
        employee.PersonalEmail = dto.PersonalEmail.Trim();
        employee.OfficialEmail = dto.OfficialEmail.Trim().ToLowerInvariant();
        employee.PhoneNumber = dto.PhoneNumber.Trim();
        employee.JoiningDate = dto.JoiningDate.Date;
        employee.EmploymentStatus = dto.EmploymentStatus;
        employee.DepartmentId = dto.DepartmentId;
        employee.DesignationId = dto.DesignationId;
        employee.Extension = dto.Extension?.Trim() ?? string.Empty;
        employee.CellPhoneNumber = dto.CellPhoneNumber?.Trim() ?? string.Empty;
        employee.Location = dto.Location?.Trim() ?? string.Empty;

        await _repository.UpdateAsync(employee, cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage, string NewStatus)> ToggleStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return (false, "Employee not found.", string.Empty);
        }

        var isCurrentlyActive = string.Equals(employee.EmploymentStatus, "Active", StringComparison.OrdinalIgnoreCase);
        var newStatus = isCurrentlyActive ? "Inactive" : "Active";
        employee.EmploymentStatus = newStatus;

        await _repository.UpdateAsync(employee, cancellationToken);
        return (true, null, newStatus);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return (false, "Employee not found.");
        }

        await _repository.DeleteAsync(employee, cancellationToken);
        return (true, null);
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetDepartmentLookupsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetDepartmentLookupsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetDesignationLookupsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetDesignationLookupsAsync(cancellationToken);
    }
}
