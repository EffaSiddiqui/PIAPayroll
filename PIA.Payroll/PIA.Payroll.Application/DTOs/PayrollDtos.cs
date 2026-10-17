using System.ComponentModel.DataAnnotations;

namespace PIA.Payroll.Application.DTOs;

public class SalaryStructureDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HouseRentAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal TotalAllowances => HouseRentAllowance + MedicalAllowance + SpecialAllowance;
    public decimal GrossSalary => BasicSalary + TotalAllowances;
    public decimal IncomeTaxDeduction { get; set; }
    public decimal ProvidentFundDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions => IncomeTaxDeduction + ProvidentFundDeduction + OtherDeductions;
    public decimal NetSalary => GrossSalary - TotalDeductions;
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; }
}

public class SalaryStructureEditDto
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Basic salary is required.")]
    [Range(0, 100000000, ErrorMessage = "Basic salary must be greater than or equal to 0.")]
    [Display(Name = "Basic Salary (PKR)")]
    public decimal BasicSalary { get; set; }

    [Range(0, 50000000, ErrorMessage = "House rent allowance cannot be negative.")]
    [Display(Name = "House Rent Allowance (PKR)")]
    public decimal HouseRentAllowance { get; set; }

    [Range(0, 50000000, ErrorMessage = "Medical allowance cannot be negative.")]
    [Display(Name = "Medical Allowance (PKR)")]
    public decimal MedicalAllowance { get; set; }

    [Range(0, 50000000, ErrorMessage = "Special allowance cannot be negative.")]
    [Display(Name = "Special / Flying Allowance (PKR)")]
    public decimal SpecialAllowance { get; set; }

    [Range(0, 50000000, ErrorMessage = "Income tax deduction cannot be negative.")]
    [Display(Name = "Income Tax Deduction (PKR)")]
    public decimal IncomeTaxDeduction { get; set; }

    [Range(0, 50000000, ErrorMessage = "Provident fund deduction cannot be negative.")]
    [Display(Name = "Provident Fund Deduction (PKR)")]
    public decimal ProvidentFundDeduction { get; set; }

    [Range(0, 50000000, ErrorMessage = "Other deductions cannot be negative.")]
    [Display(Name = "Other Deductions (PKR)")]
    public decimal OtherDeductions { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Effective From")]
    public DateTime EffectiveDate { get; set; } = DateTime.Today;

    public bool IsActive { get; set; } = true;
}

public class PayrollPeriodDto
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

public class CreatePayrollPeriodDto
{
    [Required(ErrorMessage = "Year is required.")]
    [Range(2020, 2099, ErrorMessage = "Please select a valid year between 2020 and 2099.")]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [Required(ErrorMessage = "Month is required.")]
    [Range(1, 12, ErrorMessage = "Please select a valid month (1-12).")]
    public int Month { get; set; } = DateTime.UtcNow.Month;

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Cycle Start Date")]
    public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

    [Required(ErrorMessage = "End date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Cycle End Date")]
    public DateTime EndDate { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1);

    [Display(Name = "Notes / Description")]
    [StringLength(250, ErrorMessage = "Notes cannot exceed 250 characters.")]
    public string? Notes { get; set; }
}

public class PayslipDto
{
    public int Id { get; set; }
    public int PayrollPeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string PeriodStatus { get; set; } = string.Empty;

    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string CNIC { get; set; } = string.Empty;
    public string OfficialEmail { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }

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
    public string PaymentStatus { get; set; } = "Pending";
    public DateTime? PaymentDate { get; set; }
    public DateTime GeneratedDate { get; set; }
    public string? Remarks { get; set; }
}

public class ProcessPayrollResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int EmployeesProcessed { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
}

public class PayrollDashboardSummaryDto
{
    public int TotalPeriods { get; set; }
    public int ProcessedPeriodsCount { get; set; }
    public decimal TotalNetDisbursed { get; set; }
    public int ActiveEmployeesCount { get; set; }
}
