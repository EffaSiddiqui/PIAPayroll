using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA.Payroll.Application.Interfaces;
using PIA.Payroll.Application.Services;
using PIA.Payroll.Infrastructure.Data;
using PIA.Payroll.Infrastructure.Identity;
using PIA.Payroll.Infrastructure.Repositories;
using PIA.Payroll.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Context Setup (Using Infrastructure Layer's PayrollDbContext)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<PayrollDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity Setup (Custom ApplicationUser + Roles Support, Password & Lockout Policies)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Password Policy
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout Policy
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<PayrollDbContext>();

// Application Cookie Security: Explicit ASP.NET Identity Razor Pages Routing
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// 3. Application & Infrastructure Repositories / Services
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

// 4. MVC & Razor Pages Setup with Global Security Filters
builder.Services.AddControllersWithViews(options =>
{
    // Global AutoValidateAntiforgeryTokenAttribute
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddRazorPages();

// Global Authorization: Require authenticated users by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// 5. HTTP Request Pipeline Configuration
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    // DevDataSeeder must ONLY run in Development
    using var scope = app.Services.CreateScope();
    await DevDataSeeder.SeedAsync(scope.ServiceProvider);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Middleware to enforce that inactive users must be signed out
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";

    // Bypass check for static files and Login/Register endpoints to prevent loop
    if (!path.StartsWith("/identity/account/login") &&
        !path.StartsWith("/identity/account/register") &&
        context.User.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);

        if (user == null || !user.IsActive)
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/Identity/Account/Login");
            return;
        }
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();