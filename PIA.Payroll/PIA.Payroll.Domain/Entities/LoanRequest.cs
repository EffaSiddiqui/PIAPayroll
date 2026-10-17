namespace PIA.Payroll.Domain.Entities;

public class LoanRequest
{
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public decimal Amount { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    // Status can be: "Pending", "Approved", "Rejected"
    public string Status { get; set; } = "Pending";
    
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    
    public string? AdminRemarks { get; set; }
}
