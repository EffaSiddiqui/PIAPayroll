using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA.Payroll.Domain.Entities;
using PIA.Payroll.Infrastructure.Data;
using PIA.Payroll.Infrastructure.Identity;

namespace PIA.Payroll.Web.Controllers;

[Authorize]
public class LoanController : Controller
{
    private readonly PayrollDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoanController(PayrollDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Admin/HR view all requests
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Index()
    {
        var loans = await _context.LoanRequests
            .Include(l => l.Employee)
            .OrderByDescending(l => l.RequestDate)
            .ToListAsync();
            
        return View(loans);
    }

    // Employee view their own loans
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> MyLoans()
    {
        var user = await _userManager.GetUserAsync(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.OfficialEmail == user.Email);
        
        if (employee == null) return NotFound();

        var loans = await _context.LoanRequests
            .Where(l => l.EmployeeId == employee.Id)
            .OrderByDescending(l => l.RequestDate)
            .ToListAsync();
            
        return View(loans);
    }

    // GET: Create Loan Request
    [Authorize(Roles = "Employee")]
    public IActionResult RequestLoan()
    {
        return View();
    }

    // POST: Create Loan Request
    [HttpPost]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> RequestLoan(decimal amount, string reason)
    {
        if (amount <= 0 || string.IsNullOrWhiteSpace(reason))
        {
            ModelState.AddModelError("", "Valid amount and reason are required.");
            return View();
        }

        var user = await _userManager.GetUserAsync(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.OfficialEmail == user.Email);
        
        if (employee == null) return NotFound();

        var loan = new LoanRequest
        {
            EmployeeId = employee.Id,
            Amount = amount,
            Reason = reason,
            Status = "Pending",
            RequestDate = DateTime.UtcNow
        };

        _context.LoanRequests.Add(loan);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Loan request submitted successfully.";
        return RedirectToAction(nameof(MyLoans));
    }

    // POST: Update Loan Status (Approve/Reject)
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? remarks)
    {
        var loan = await _context.LoanRequests.FindAsync(id);
        if (loan == null) return NotFound();

        if (status == "Approved" || status == "Rejected")
        {
            loan.Status = status;
            loan.AdminRemarks = remarks;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Loan {status.ToLower()} successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}
