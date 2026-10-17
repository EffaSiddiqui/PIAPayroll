using Microsoft.AspNetCore.Identity;

namespace PIA.Payroll.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }

    public bool IsActive { get; set; } = true;
}