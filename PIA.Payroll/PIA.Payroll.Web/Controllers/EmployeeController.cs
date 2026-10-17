using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PIA.Payroll.Application.DTOs;
using PIA.Payroll.Application.Interfaces;
using PIA.Payroll.Infrastructure.Identity;
using PIA.Payroll.Web.ViewModels;

namespace PIA.Payroll.Web.Controllers;

[Authorize]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        IEmployeeService employeeService,
        UserManager<ApplicationUser> userManager,
        ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: /Employee
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Index([FromQuery] EmployeeFilterDto filter)
    {
        var employees = await _employeeService.GetAllEmployeesAsync(filter);
        var departments = await _employeeService.GetDepartmentLookupsAsync();
        var designations = await _employeeService.GetDesignationLookupsAsync();

        var viewModel = new EmployeeListViewModel
        {
            Employees = employees,
            Filter = filter,
            Departments = departments,
            Designations = designations
        };

        return View(viewModel);
    }

    // GET: /Employee/Details/5
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Details(int id)
    {
        var employee = await _employeeService.GetEmployeeDetailsAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // GET: /Employee/Create
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var viewModel = new EmployeeCreateViewModel
        {
            Departments = await _employeeService.GetDepartmentLookupsAsync(),
            Designations = await _employeeService.GetDesignationLookupsAsync()
        };

        return View(viewModel);
    }

    // POST: /Employee/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(EmployeeCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Departments = await _employeeService.GetDepartmentLookupsAsync();
            model.Designations = await _employeeService.GetDesignationLookupsAsync();
            return View(model);
        }

        var (success, errorMessage, employeeId) = await _employeeService.CreateEmployeeAsync(model.Employee);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to register employee.");
            model.Departments = await _employeeService.GetDepartmentLookupsAsync();
            model.Designations = await _employeeService.GetDesignationLookupsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = $"Personnel profile for '{model.Employee.FirstName} {model.Employee.LastName}' ({model.Employee.EmployeeCode}) created successfully.";
        _logger.LogInformation("Employee {Code} registered by {User}", model.Employee.EmployeeCode, User.Identity?.Name);

        return RedirectToAction(nameof(Details), new { id = employeeId });
    }

    // GET: /Employee/Edit/5
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Edit(int id)
    {
        var editDto = await _employeeService.GetEmployeeForEditAsync(id);
        if (editDto == null)
        {
            return NotFound();
        }

        var viewModel = new EmployeeEditViewModel
        {
            Employee = editDto,
            Departments = await _employeeService.GetDepartmentLookupsAsync(),
            Designations = await _employeeService.GetDesignationLookupsAsync()
        };

        return View(viewModel);
    }

    // POST: /Employee/Edit/5
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
    {
        if (id != model.Employee.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.Departments = await _employeeService.GetDepartmentLookupsAsync();
            model.Designations = await _employeeService.GetDesignationLookupsAsync();
            return View(model);
        }

        var (success, errorMessage) = await _employeeService.UpdateEmployeeAsync(model.Employee);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to update employee.");
            model.Departments = await _employeeService.GetDepartmentLookupsAsync();
            model.Designations = await _employeeService.GetDesignationLookupsAsync();
            return View(model);
        }

        // If status changed to Inactive or Active, sync corresponding Identity user if one exists
        var identityUser = await _userManager.FindByEmailAsync(model.Employee.OfficialEmail);
        if (identityUser != null)
        {
            var isEmpActive = string.Equals(model.Employee.EmploymentStatus, "Active", StringComparison.OrdinalIgnoreCase);
            if (identityUser.IsActive != isEmpActive)
            {
                identityUser.IsActive = isEmpActive;
                await _userManager.UpdateAsync(identityUser);
            }
        }

        TempData["SuccessMessage"] = $"Employee record '{model.Employee.FirstName} {model.Employee.LastName}' updated successfully.";
        _logger.LogInformation("Employee {Id} updated by {User}", id, User.Identity?.Name);

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Employee/ToggleStatus/5
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> ToggleStatus(int id, [FromForm] string? returnUrl = null)
    {
        var (success, errorMessage, newStatus) = await _employeeService.ToggleStatusAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Unable to change employee status.";
            return RedirectBack(returnUrl, id);
        }

        // Sync with Identity user to trigger immediate signout if deactivated (Phase 1 hardening)
        var employee = await _employeeService.GetEmployeeDetailsAsync(id);
        if (employee != null)
        {
            var identityUser = await _userManager.FindByEmailAsync(employee.OfficialEmail);
            if (identityUser != null)
            {
                identityUser.IsActive = (newStatus == "Active");
                await _userManager.UpdateAsync(identityUser);
            }
        }

        TempData["SuccessMessage"] = $"Employee status changed to '{newStatus}' successfully.";
        _logger.LogInformation("Employee {Id} status updated to {Status} by {User}", id, newStatus, User.Identity?.Name);

        return RedirectBack(returnUrl, id);
    }

    private IActionResult RedirectBack(string? returnUrl, int employeeId)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { id = employeeId });
    }

    // POST: /Employee/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, [FromForm] string? returnUrl = null)
    {
        var employee = await _employeeService.GetEmployeeDetailsAsync(id);
        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee not found.";
            return RedirectToAction(nameof(Index));
        }

        var (success, errorMessage) = await _employeeService.DeleteEmployeeAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Unable to delete employee.";
            return RedirectBack(returnUrl, id);
        }

        // Also delete identity user if exists
        var identityUser = await _userManager.FindByEmailAsync(employee.OfficialEmail);
        if (identityUser != null)
        {
            await _userManager.DeleteAsync(identityUser);
        }

        TempData["SuccessMessage"] = $"Employee record for '{employee.FirstName} {employee.LastName}' deleted successfully.";
        _logger.LogInformation("Employee {Id} deleted by {User}", id, User.Identity?.Name);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Employee/MyProfile
    [Authorize]
    public async Task<IActionResult> MyProfile()
    {
        // For employee, find their details by email
        var userEmail = User.Identity?.Name;
        if (string.IsNullOrEmpty(userEmail))
        {
            return NotFound();
        }

        // Fetch the employee using their email. Since IEmployeeService might not have a direct GetByEmail, 
        // we can use GetAllEmployeesAsync and filter by email.
        var all = await _employeeService.GetAllEmployeesAsync(new EmployeeFilterDto { SearchTerm = userEmail });
        var empInfo = all.FirstOrDefault(e => e.OfficialEmail == userEmail);

        if (empInfo == null)
        {
            return NotFound("Employee profile not found for the logged-in user.");
        }

        var employee = await _employeeService.GetEmployeeDetailsAsync(empInfo.Id);
        if (employee == null)
        {
            return NotFound();
        }

        // We can reuse the Details view
        return View("Details", employee);
    }
}
