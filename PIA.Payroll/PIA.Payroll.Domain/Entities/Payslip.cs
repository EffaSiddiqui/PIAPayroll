namespace PIA.Payroll.Domain.Entities;

public class Payslip
{
    public int Id { get; set; }

    public int PayrollPeriodId { get; set; }

    public int EmployeeId { get; set; }

    public decimal BasicSalary { get; set; }

    public decimal HouseRentAllowance { get; set; }

    public decimal MedicalAllowance { get; set; }

    public decimal SpecialAllowance { get; set; }

    public decimal TotalAllowances { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal IncomeTaxDeduction { get; set; }

    public decimal ProvidentFundDeduction { get; set; }

    public decimal OtherDeductions { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal NetSalary { get; set; }

    public string PaymentStatus { get; set; } = "Pending"; // "Pending", "Paid"

    public DateTime? PaymentDate { get; set; }

    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    public string? Remarks { get; set; }
}
