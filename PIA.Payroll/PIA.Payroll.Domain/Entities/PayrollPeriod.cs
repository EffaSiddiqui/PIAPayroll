namespace PIA.Payroll.Domain.Entities;

public class PayrollPeriod
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public string PeriodName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = "Draft"; // "Draft", "Processed", "Paid"

    public DateTime? ProcessedDate { get; set; }

    public string? ProcessedBy { get; set; }

    public int TotalEmployeesProcessed { get; set; }

    public decimal TotalGrossSalary { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal TotalNetSalary { get; set; }

    public string? Notes { get; set; }
}
