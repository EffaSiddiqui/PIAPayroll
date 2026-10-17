using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA.Payroll.Infrastructure.Data;
using PIA.Payroll.Web.Models;
using PIA.Payroll.Web.ViewModels;

namespace PIA.Payroll.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PayrollDbContext _context;
    private readonly IHostEnvironment _env;

    public HomeController(
        ILogger<HomeController> logger,
        PayrollDbContext context,
        IHostEnvironment env)
    {
        _logger = logger;
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var isDev = _env.IsDevelopment();
        var totalEmployees = await _context.Employees.CountAsync();
        var totalDepartments = await _context.Departments.CountAsync();
        var totalDesignations = await _context.Designations.CountAsync();

        var viewModel = new DashboardViewModel
        {
            IsDevelopment = isDev,
            HasData = totalEmployees > 0,
            TotalEmployees = totalEmployees,
            ActiveDepartments = totalDepartments,
            TotalDesignations = totalDesignations,
            TotalEstimatedPayroll = isDev && totalEmployees > 0 ? 4_850_000m : 0m,
            PayrollCyclePeriod = DateTime.UtcNow.ToString("MMMM yyyy")
        };

        return View(viewModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}