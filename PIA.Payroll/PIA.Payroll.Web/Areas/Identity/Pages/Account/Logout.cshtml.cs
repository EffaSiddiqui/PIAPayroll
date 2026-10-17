using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PIA.Payroll.Infrastructure.Identity;

namespace PIA.Payroll.Web.Areas.Identity.Pages.Account;

[Authorize]
[ValidateAntiForgeryToken]
public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Authenticated user logged out securely.");

        if (returnUrl != null && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect("/Identity/Account/Login");
    }

    public IActionResult OnGet()
    {
        // Disallow unauthenticated or GET-based logouts (CSRF prevention)
        return LocalRedirect("/Identity/Account/Login");
    }
}
