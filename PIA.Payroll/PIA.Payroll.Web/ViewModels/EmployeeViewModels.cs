using PIA.Payroll.Application.DTOs;

namespace PIA.Payroll.Web.ViewModels;

public class EmployeeListViewModel
{
    public IReadOnlyList<EmployeeListItemDto> Employees { get; set; } = [];
    public EmployeeFilterDto Filter { get; set; } = new();
    public IReadOnlyList<LookupItemDto> Departments { get; set; } = [];
    public IReadOnlyList<LookupItemDto> Designations { get; set; } = [];
    public int TotalCount => Employees.Count;
    public int ActiveCount => Employees.Count(e => e.IsActive);
    public int InactiveCount => Employees.Count(e => !e.IsActive);
}

public class EmployeeCreateViewModel
{
    public EmployeeCreateDto Employee { get; set; } = new();
    public IReadOnlyList<LookupItemDto> Departments { get; set; } = [];
    public IReadOnlyList<LookupItemDto> Designations { get; set; } = [];
}

public class EmployeeEditViewModel
{
    public EmployeeEditDto Employee { get; set; } = new();
    public IReadOnlyList<LookupItemDto> Departments { get; set; } = [];
    public IReadOnlyList<LookupItemDto> Designations { get; set; } = [];
}
