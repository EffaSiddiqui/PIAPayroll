using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PIA.Payroll.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
    }

    [HttpPost]
    public IActionResult Login(string Username, string Password)
    {
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }
}
