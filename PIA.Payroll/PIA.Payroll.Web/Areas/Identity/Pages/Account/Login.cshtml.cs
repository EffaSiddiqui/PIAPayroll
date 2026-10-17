using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PIA.Payroll.Infrastructure.Identity;

namespace PIA.Payroll.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Email or Username is required.")]
        [Display(Name = "Email or Username")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember this device")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear existing external cookie to ensure clean login
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // 1. User lookup
        var user = await _userManager.FindByEmailAsync(Input.Email)
                   ?? await _userManager.FindByNameAsync(Input.Email);

        if (user == null)
        {
            // Generic message for nonexistent user
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        // 2. Check if user is inactive
        if (!user.IsActive)
        {
            // Inactive users must be signed out immediately
            await _signInManager.SignOutAsync();
            // Generic message for inactive account (do not disclose account state)
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        // 3. PasswordSignInAsync with lockoutOnFailure: true
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName ?? user.Email!,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Double-check active state
            if (!user.IsActive)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            _logger.LogInformation("User {Email} successfully authenticated.", user.Email);

            // 4. Preserve local returnUrl validation
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return LocalRedirect("~/");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account is locked out.", user.Email);
            ModelState.AddModelError(string.Empty, "Account locked due to multiple failed attempts. Please try again later.");
            return Page();
        }

        // Generic error for wrong password or other failure
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}
