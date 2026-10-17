using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PIA.Payroll.Domain.Entities;
using PIA.Payroll.Infrastructure.Identity;

namespace PIA.Payroll.Infrastructure.Data;

public class PayrollDbContext : IdentityDbContext<ApplicationUser>
{
    public PayrollDbContext(
        DbContextOptions<PayrollDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Designation> Designations => Set<Designation>();

    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();

    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();

    public DbSet<Payslip> Payslips => Set<Payslip>();

    public DbSet<LoanRequest> LoanRequests => Set<LoanRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SalaryStructure>(b =>
        {
            b.HasIndex(s => s.EmployeeId);
            b.Property(s => s.BasicSalary).HasPrecision(18, 2);
            b.Property(s => s.HouseRentAllowance).HasPrecision(18, 2);
            b.Property(s => s.MedicalAllowance).HasPrecision(18, 2);
            b.Property(s => s.SpecialAllowance).HasPrecision(18, 2);
            b.Property(s => s.IncomeTaxDeduction).HasPrecision(18, 2);
            b.Property(s => s.ProvidentFundDeduction).HasPrecision(18, 2);
            b.Property(s => s.OtherDeductions).HasPrecision(18, 2);
        });

        builder.Entity<PayrollPeriod>(b =>
        {
            b.HasIndex(p => new { p.Year, p.Month }).IsUnique();
            b.Property(p => p.TotalGrossSalary).HasPrecision(18, 2);
            b.Property(p => p.TotalDeductions).HasPrecision(18, 2);
            b.Property(p => p.TotalNetSalary).HasPrecision(18, 2);
        });

        builder.Entity<Payslip>(b =>
        {
            b.HasIndex(p => new { p.PayrollPeriodId, p.EmployeeId }).IsUnique();
            b.Property(p => p.BasicSalary).HasPrecision(18, 2);
            b.Property(p => p.HouseRentAllowance).HasPrecision(18, 2);
            b.Property(p => p.MedicalAllowance).HasPrecision(18, 2);
            b.Property(p => p.SpecialAllowance).HasPrecision(18, 2);
            b.Property(p => p.TotalAllowances).HasPrecision(18, 2);
            b.Property(p => p.GrossSalary).HasPrecision(18, 2);
            b.Property(p => p.IncomeTaxDeduction).HasPrecision(18, 2);
            b.Property(p => p.ProvidentFundDeduction).HasPrecision(18, 2);
            b.Property(p => p.OtherDeductions).HasPrecision(18, 2);
            b.Property(p => p.TotalDeductions).HasPrecision(18, 2);
            b.Property(p => p.NetSalary).HasPrecision(18, 2);
        });

        builder.Entity<LoanRequest>(b =>
        {
            b.Property(l => l.Amount).HasPrecision(18, 2);
        });
    }
}