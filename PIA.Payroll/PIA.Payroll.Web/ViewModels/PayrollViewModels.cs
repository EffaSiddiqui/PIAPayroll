using PIA.Payroll.Application.DTOs;

namespace PIA.Payroll.Web.ViewModels;

public class PayrollDashboardViewModel
{
    public PayrollDashboardSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<PayrollPeriodDto> Periods { get; set; } = [];
    public bool IsAdminOrHr { get; set; }
}

public class PayrollPeriodDetailsViewModel
{
    public PayrollPeriodDto Period { get; set; } = new();
    public IReadOnlyList<PayslipDto> Payslips { get; set; } = [];
    public bool IsAdminOrHr { get; set; }
}

public class SalaryStructuresListViewModel
{
    public IReadOnlyList<SalaryStructureDto> Structures { get; set; } = [];
}

public class MyPayslipsViewModel
{
    public EmployeeDetailsDto? Employee { get; set; }
    public IReadOnlyList<PayslipDto> Payslips { get; set; } = [];
    public decimal TotalNetYtd => Payslips.Where(p => p.PaymentStatus == "Paid").Sum(p => p.NetSalary);
    public decimal TotalGrossYtd => Payslips.Where(p => p.PaymentStatus == "Paid").Sum(p => p.GrossSalary);
}
