using Microsoft.EntityFrameworkCore;
using PIA.Payroll.Application.DTOs;
using PIA.Payroll.Application.Interfaces;
using PIA.Payroll.Domain.Entities;
using PIA.Payroll.Infrastructure.Data;

namespace PIA.Payroll.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly PayrollDbContext _context;

    public EmployeeRepository(PayrollDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EmployeeListItemDto>> GetEmployeesAsync(EmployeeFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = from emp in _context.Employees.AsNoTracking()
                    join dept in _context.Departments.AsNoTracking() on emp.DepartmentId equals dept.Id into deptGroup
                    from dept in deptGroup.DefaultIfEmpty()
                    join desig in _context.Designations.AsNoTracking() on emp.DesignationId equals desig.Id into desigGroup
                    from desig in desigGroup.DefaultIfEmpty()
                    select new
                    {
                        emp,
                        DepartmentName = dept != null ? dept.Name : null,
                        DesignationName = desig != null ? desig.Name : null
                    };

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.emp.EmployeeCode.ToLower().Contains(term) ||
                x.emp.FirstName.ToLower().Contains(term) ||
                x.emp.LastName.ToLower().Contains(term) ||
                x.emp.CNIC.ToLower().Contains(term) ||
                x.emp.OfficialEmail.ToLower().Contains(term) ||
                x.emp.PhoneNumber.ToLower().Contains(term));
        }

        if (filter.DepartmentId.HasValue && filter.DepartmentId > 0)
        {
            query = query.Where(x => x.emp.DepartmentId == filter.DepartmentId.Value);
        }

        if (filter.DesignationId.HasValue && filter.DesignationId > 0)
        {
            query = query.Where(x => x.emp.DesignationId == filter.DesignationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status) && !string.Equals(filter.Status, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.emp.EmploymentStatus == filter.Status);
        }

        return await query
            .OrderBy(x => x.emp.EmployeeCode)
            .Select(x => new EmployeeListItemDto
            {
                Id = x.emp.Id,
                EmployeeCode = x.emp.EmployeeCode,
                FirstName = x.emp.FirstName,
                LastName = x.emp.LastName,
                FullName = x.emp.FirstName + " " + x.emp.LastName,
                FatherName = x.emp.FatherName,
                CNIC = x.emp.CNIC,
                OfficialEmail = x.emp.OfficialEmail,
                PhoneNumber = x.emp.PhoneNumber,
                DepartmentId = x.emp.DepartmentId,
                DepartmentName = x.DepartmentName,
                DesignationId = x.emp.DesignationId,
                DesignationName = x.DesignationName,
                JoiningDate = x.emp.JoiningDate,
                EmploymentStatus = x.emp.EmploymentStatus,
                Extension = x.emp.Extension,
                CellPhoneNumber = x.emp.CellPhoneNumber,
                Location = x.emp.Location
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeDetailsDto?> GetEmployeeDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var query = from emp in _context.Employees.AsNoTracking()
                    where emp.Id == id
                    join dept in _context.Departments.AsNoTracking() on emp.DepartmentId equals dept.Id into deptGroup
                    from dept in deptGroup.DefaultIfEmpty()
                    join desig in _context.Designations.AsNoTracking() on emp.DesignationId equals desig.Id into desigGroup
                    from desig in desigGroup.DefaultIfEmpty()
                    select new EmployeeDetailsDto
                    {
                        Id = emp.Id,
                        EmployeeCode = emp.EmployeeCode,
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        FatherName = emp.FatherName,
                        CNIC = emp.CNIC,
                        DateOfBirth = emp.DateOfBirth,
                        Gender = emp.Gender,
                        PersonalEmail = emp.PersonalEmail,
                        OfficialEmail = emp.OfficialEmail,
                        PhoneNumber = emp.PhoneNumber,
                        JoiningDate = emp.JoiningDate,
                        EmploymentStatus = emp.EmploymentStatus,
                        DepartmentId = emp.DepartmentId,
                        DepartmentName = dept != null ? dept.Name : null,
                        DesignationId = emp.DesignationId,
                        DesignationName = desig != null ? desig.Name : null,
                        Extension = emp.Extension,
                        CellPhoneNumber = emp.CellPhoneNumber,
                        Location = emp.Location
                    };

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string employeeCode, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        return await query.AnyAsync(e => e.EmployeeCode.ToLower() == employeeCode.ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsByCnicAsync(string cnic, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        return await query.AnyAsync(e => e.CNIC == cnic, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        return await query.AnyAsync(e => e.OfficialEmail.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetDepartmentLookupsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new LookupItemDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetDesignationLookupsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Designations
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new LookupItemDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code
            })
            .ToListAsync(cancellationToken);
    }
}
