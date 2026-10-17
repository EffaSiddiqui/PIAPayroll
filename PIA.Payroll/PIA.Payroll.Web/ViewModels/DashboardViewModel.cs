namespace PIA.Payroll.Web.ViewModels;

public class DashboardViewModel
{
    public bool IsDevelopment { get; set; }
    public bool HasData { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveDepartments { get; set; }
    public int TotalDesignations { get; set; }
    public decimal TotalEstimatedPayroll { get; set; }
    public string PayrollCyclePeriod { get; set; } = string.Empty;
}
