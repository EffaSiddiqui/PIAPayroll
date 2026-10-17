using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PIA.Payroll.Application.DTOs;
using PIA.Payroll.Application.Interfaces;
using PIA.Payroll.Infrastructure.Identity;
using PIA.Payroll.Web.ViewModels;

namespace PIA.Payroll.Web.Controllers;

[Authorize]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly IEmployeeService _employeeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PayrollController> _logger;

    public PayrollController(
        IPayrollService payrollService,
        IEmployeeService employeeService,
        UserManager<ApplicationUser> userManager,
        ILogger<PayrollController> logger)
    {
        _payrollService = payrollService;
        _employeeService = employeeService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: /Payroll
    [Authorize(Roles = "Admin,HR,Employee")]
    public async Task<IActionResult> Index()
    {
        // If employee user without administrative privileges, route directly to personal payslips
        if (!User.IsInRole("Admin") && !User.IsInRole("HR"))
        {
            return RedirectToAction(nameof(MyPayslips));
        }

        var periods = await _payrollService.GetAllPeriodsAsync();
        var summary = await _payrollService.GetDashboardSummaryAsync();

        var viewModel = new PayrollDashboardViewModel
        {
            Periods = periods,
            Summary = summary,
            IsAdminOrHr = true
        };

        return View(viewModel);
    }

    // GET: /Payroll/CreatePeriod
    [Authorize(Roles = "Admin,HR")]
    public IActionResult CreatePeriod()
    {
        var model = new CreatePayrollPeriodDto
        {
            Year = DateTime.UtcNow.Year,
            Month = DateTime.UtcNow.Month,
            StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
            EndDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1)
        };
        return View(model);
    }

    // POST: /Payroll/CreatePeriod
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreatePeriod(CreatePayrollPeriodDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage, periodId) = await _payrollService.CreatePeriodAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to create payroll period.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Payroll period initialized successfully. You can now execute salary processing for active staff.";
        _logger.LogInformation("Payroll cycle {Id} created by {User}", periodId, User.Identity?.Name);

        return RedirectToAction(nameof(Details), new { id = periodId });
    }

    // GET: /Payroll/Details/5
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Details(int id)
    {
        var period = await _payrollService.GetPeriodByIdAsync(id);
        if (period == null)
        {
            return NotFound();
        }

        var payslips = await _payrollService.GetPayslipsForPeriodAsync(id);

        var viewModel = new PayrollPeriodDetailsViewModel
        {
            Period = period,
            Payslips = payslips,
            IsAdminOrHr = true
        };

        return View(viewModel);
    }

    // POST: /Payroll/ProcessPeriod/5
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> ProcessPeriod(int id)
    {
        var processedBy = User.Identity?.Name ?? "HR Administrator";
        var (success, errorMessage, result) = await _payrollService.ProcessPeriodAsync(id, processedBy);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Failed to process payroll period.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["SuccessMessage"] = $"Payroll run executed successfully: {result?.EmployeesProcessed} active personnel processed. Gross: PKR {result?.TotalGross:N0}, Net Outlay: PKR {result?.TotalNet:N0}.";
        _logger.LogInformation("Payroll period {Id} processed by {User}", id, User.Identity?.Name);

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Payroll/MarkAsPaid/5
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var (success, errorMessage) = await _payrollService.MarkPeriodAsPaidAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Unable to finalize and mark period as Paid.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["SuccessMessage"] = "Payroll cycle finalized and marked as PAID. Individual staff accounts updated.";
        _logger.LogInformation("Payroll period {Id} marked as Paid by {User}", id, User.Identity?.Name);

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /Payroll/TogglePayslipStatus/5
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> TogglePayslipStatus(int id)
    {
        var payslip = await _payrollService.GetPayslipDetailsAsync(id);
        if (payslip == null) return NotFound();

        // Normally we'd do this via a service method, but we can do it via DbContext directly if we have to, 
        // wait, I don't have DbContext injected in PayrollController. Let's just create a quick direct update or 
        // use IPayrollService if there is a method. Since there isn't, I will use IServiceProvider to get context.
        var context = HttpContext.RequestServices.GetRequiredService<PIA.Payroll.Infrastructure.Data.PayrollDbContext>();
        var dbPayslip = await context.Payslips.FindAsync(id);
        if (dbPayslip != null)
        {
            dbPayslip.PaymentStatus = dbPayslip.PaymentStatus == "Paid" ? "Pending" : "Paid";
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Payslip payment status updated to {dbPayslip.PaymentStatus}.";
        }

        // Redirect back to period details
        return RedirectToAction(nameof(Details), new { id = payslip.PayrollPeriodId });
    }

    // GET: /Payroll/SalaryStructures
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> SalaryStructures()
    {
        var structures = await _payrollService.GetAllSalaryStructuresAsync();
        var model = new SalaryStructuresListViewModel
        {
            Structures = structures
        };
        return View(model);
    }

    // GET: /Payroll/EditSalaryStructure/5
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> EditSalaryStructure(int employeeId)
    {
        var model = await _payrollService.GetSalaryStructureForEditAsync(employeeId);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // POST: /Payroll/EditSalaryStructure/5
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> EditSalaryStructure(SalaryStructureEditDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, errorMessage) = await _payrollService.SaveSalaryStructureAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to save salary configuration.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"Salary parameters for {model.EmployeeName} ({model.EmployeeCode}) updated successfully.";
        _logger.LogInformation("Salary structure for employee {EmpId} updated by {User}", model.EmployeeId, User.Identity?.Name);

        return RedirectToAction(nameof(SalaryStructures));
    }

    // GET: /Payroll/Payslip/5
    [Authorize(Roles = "Admin,HR,Employee")]
    public async Task<IActionResult> Payslip(int id)
    {
        var payslip = await _payrollService.GetPayslipDetailsAsync(id);
        if (payslip == null)
        {
            return NotFound();
        }

        // STRICT SECURITY / DATA ISOLATION:
        // Employee role can strictly view only their own payslip
        if (!User.IsInRole("Admin") && !User.IsInRole("HR"))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var isOwner = (currentUser.EmployeeId.HasValue && currentUser.EmployeeId.Value == payslip.EmployeeId) ||
                          string.Equals(currentUser.Email, payslip.OfficialEmail, StringComparison.OrdinalIgnoreCase);

            if (!isOwner)
            {
                _logger.LogWarning("Security violation: User {User} attempted unauthorized access to payslip {Id} belonging to employee {EmpId}",
                    User.Identity?.Name, id, payslip.EmployeeId);
                return Forbid();
            }
        }

        return View(payslip);
    }

    // GET: /Payroll/MyPayslips
    [Authorize(Roles = "Admin,HR,Employee")]
    public async Task<IActionResult> MyPayslips()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Challenge();
        }

        EmployeeDetailsDto? employee = null;

        if (currentUser.EmployeeId.HasValue)
        {
            employee = await _employeeService.GetEmployeeDetailsAsync(currentUser.EmployeeId.Value);
        }
        else if (!string.IsNullOrEmpty(currentUser.Email))
        {
            // Fallback lookup by email
            var allEmployees = await _employeeService.GetAllEmployeesAsync(new EmployeeFilterDto { SearchTerm = currentUser.Email });
            var matched = allEmployees.FirstOrDefault(e => string.Equals(e.OfficialEmail, currentUser.Email, StringComparison.OrdinalIgnoreCase));
            if (matched != null)
            {
                employee = await _employeeService.GetEmployeeDetailsAsync(matched.Id);
            }
        }

        var viewModel = new MyPayslipsViewModel
        {
            Employee = employee,
            Payslips = employee != null ? await _payrollService.GetEmployeePayslipsAsync(employee.Id) : []
        };

        return View(viewModel);
    }
    // GET: /Payroll/FlightAllowances
    [Authorize(Roles = "Admin,HR,Employee")]
    public IActionResult FlightAllowances()
    {
        return View();
    }

    // GET: /Payroll/TaxSettings
    [Authorize(Roles = "Admin,HR,Employee")]
    public IActionResult TaxSettings()
    {
        return View();
    }

    // GET: /Payroll/Disbursement
    [Authorize(Roles = "Admin,HR,Employee")]
    public IActionResult Disbursement()
    {
        return View();
    }
}
