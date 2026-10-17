using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PIA.Payroll.Domain.Entities;
using PIA.Payroll.Infrastructure.Data;
using PIA.Payroll.Infrastructure.Identity;

namespace PIA.Payroll.Web.Data;

public static class DevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        // STRICT SAFETY CHECK: DevDataSeeder must ONLY run in Development
        var env = serviceProvider.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment())
        {
            return; // Never seed demo users/data in Production
        }

        var context = serviceProvider.GetRequiredService<PayrollDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created/migrated
        await context.Database.MigrateAsync();

        // 1. Seed Roles
        string[] roles = ["Admin", "HR", "Employee"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Departments
        if (!await context.Departments.AnyAsync())
        {
            var departments = new List<Department>
            {
                new() { Name = "Flight Operations", Code = "FLT-OPS", IsActive = true },
                new() { Name = "Information Technology", Code = "IT-SYS", IsActive = true },
                new() { Name = "Human Resources", Code = "HR-ADM", IsActive = true },
                new() { Name = "Engineering & Maintenance", Code = "ENG-MNT", IsActive = true }
            };
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        // 3. Seed Designations
        if (!await context.Designations.AnyAsync())
        {
            var designations = new List<Designation>
            {
                new() { Name = "Captain / Senior Commander", Code = "CAPT-01", IsActive = true },
                new() { Name = "First Officer", Code = "FO-01", IsActive = true },
                new() { Name = "Lead Systems Architect", Code = "TECH-01", IsActive = true },
                new() { Name = "Senior Payroll Specialist", Code = "PAY-01", IsActive = true }
            };
            await context.Designations.AddRangeAsync(designations);
            await context.SaveChangesAsync();
        }

        // 4. Seed Employees
        if (!await context.Employees.AnyAsync())
        {
            var flightDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "FLT-OPS");
            var itDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "IT-SYS");
            var captDesig = await context.Designations.FirstOrDefaultAsync(d => d.Code == "CAPT-01");
            var techDesig = await context.Designations.FirstOrDefaultAsync(d => d.Code == "TECH-01");

            var employees = new List<Employee>
            {
                new()
                {
                    EmployeeCode = "PIA-8801",
                    FirstName = "Tariq",
                    LastName = "Mahmood",
                    FatherName = "Mahmood Ahmed",
                    CNIC = "42101-1234567-1",
                    DateOfBirth = new DateTime(1982, 5, 14),
                    Gender = "Male",
                    PersonalEmail = "tariq.personal@example.com",
                    OfficialEmail = "employee@piapayroll.local",
                    PhoneNumber = "+92-300-1234567",
                    JoiningDate = new DateTime(2015, 3, 1),
                    EmploymentStatus = "Active",
                    DepartmentId = flightDept?.Id,
                    DesignationId = captDesig?.Id
                },
                new()
                {
                    EmployeeCode = "PIA-9204",
                    FirstName = "Ayesha",
                    LastName = "Khan",
                    FatherName = "Rashid Khan",
                    CNIC = "42201-7654321-2",
                    DateOfBirth = new DateTime(1990, 8, 22),
                    Gender = "Female",
                    PersonalEmail = "ayesha.personal@example.com",
                    OfficialEmail = "hr@piapayroll.local",
                    PhoneNumber = "+92-321-9876543",
                    JoiningDate = new DateTime(2018, 7, 15),
                    EmploymentStatus = "Active",
                    DepartmentId = itDept?.Id,
                    DesignationId = techDesig?.Id
                }
            };
            await context.Employees.AddRangeAsync(employees);
            await context.SaveChangesAsync();
        }

        // 5. Seed Users
        var tariqEmp = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == "PIA-8801");
        var ayeshaEmp = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == "PIA-9204");

        // Demo Admin User
        await EnsureUserAsync(
            userManager,
            email: "admin@piapayroll.local",
            password: "Admin@123",
            fullName: "System Administrator",
            role: "Admin",
            isActive: true,
            employeeId: null);

        // Demo HR User
        await EnsureUserAsync(
            userManager,
            email: "hr@piapayroll.local",
            password: "Hr@12345",
            fullName: "Ayesha Khan (HR Lead)",
            role: "HR",
            isActive: true,
            employeeId: ayeshaEmp?.Id);

        // Demo Employee User
        await EnsureUserAsync(
            userManager,
            email: "employee@piapayroll.local",
            password: "Employee@123",
            fullName: "Capt. Tariq Mahmood",
            role: "Employee",
            isActive: true,
            employeeId: tariqEmp?.Id);

        // Demo Inactive User (for testing inactive login security)
        await EnsureUserAsync(
            userManager,
            email: "inactive@piapayroll.local",
            password: "Inactive@123",
            fullName: "Inactive Demo User",
            role: "Employee",
            isActive: false,
            employeeId: null);

        // 6. Seed Salary Structures for Demo Employees
        if (!await context.SalaryStructures.AnyAsync() && tariqEmp != null && ayeshaEmp != null)
        {
            var salaryStructures = new List<SalaryStructure>
            {
                new()
                {
                    EmployeeId = tariqEmp.Id,
                    BasicSalary = 180_000m,
                    HouseRentAllowance = 81_000m,
                    MedicalAllowance = 18_000m,
                    SpecialAllowance = 45_000m,
                    IncomeTaxDeduction = 22_000m,
                    ProvidentFundDeduction = 14_400m,
                    OtherDeductions = 0m,
                    EffectiveDate = new DateTime(2026, 1, 1),
                    IsActive = true
                },
                new()
                {
                    EmployeeId = ayeshaEmp.Id,
                    BasicSalary = 120_000m,
                    HouseRentAllowance = 54_000m,
                    MedicalAllowance = 12_000m,
                    SpecialAllowance = 15_000m,
                    IncomeTaxDeduction = 11_000m,
                    ProvidentFundDeduction = 9_600m,
                    OtherDeductions = 0m,
                    EffectiveDate = new DateTime(2026, 1, 1),
                    IsActive = true
                }
            };
            await context.SalaryStructures.AddRangeAsync(salaryStructures);
            await context.SaveChangesAsync();
        }

        // 7. Seed Initial Demo Payroll Period
        if (!await context.PayrollPeriods.AnyAsync())
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var periodName = new DateTime(currentYear, currentMonth, 1).ToString("MMMM yyyy");

            var demoPeriod = new PayrollPeriod
            {
                Year = currentYear,
                Month = currentMonth,
                PeriodName = periodName,
                StartDate = new DateTime(currentYear, currentMonth, 1),
                EndDate = new DateTime(currentYear, currentMonth, 1).AddMonths(1).AddDays(-1),
                Status = "Draft",
                Notes = "Initial monthly payroll processing cycle for corporate personnel."
            };
            await context.PayrollPeriods.AddAsync(demoPeriod);
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role,
        bool isActive,
        int? employeeId)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                IsActive = isActive,
                EmployeeId = employeeId
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
        else
        {
            // Sync active flag for testing consistency in dev
            if (user.IsActive != isActive || user.FullName != fullName)
            {
                user.IsActive = isActive;
                user.FullName = fullName;
                await userManager.UpdateAsync(user);
            }
        }
    }
}
