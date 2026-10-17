namespace PIA.Payroll.Domain.Entities;

public class SalaryStructure
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public decimal BasicSalary { get; set; }

    public decimal HouseRentAllowance { get; set; }

    public decimal MedicalAllowance { get; set; }

    public decimal SpecialAllowance { get; set; }

    public decimal IncomeTaxDeduction { get; set; }

    public decimal ProvidentFundDeduction { get; set; }

    public decimal OtherDeductions { get; set; }

    public DateTime EffectiveDate { get; set; } = DateTime.Today;

    public bool IsActive { get; set; } = true;

    public decimal TotalAllowances => HouseRentAllowance + MedicalAllowance + SpecialAllowance;

    public decimal TotalDeductions => IncomeTaxDeduction + ProvidentFundDeduction + OtherDeductions;

    public decimal GrossSalary => BasicSalary + TotalAllowances;

    public decimal NetSalary => GrossSalary - TotalDeductions;
}
