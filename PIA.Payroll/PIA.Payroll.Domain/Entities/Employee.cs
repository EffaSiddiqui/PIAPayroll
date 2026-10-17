namespace PIA.Payroll.Domain.Entities;

public class Employee
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FatherName { get; set; } = string.Empty;

    public string CNIC { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string PersonalEmail { get; set; } = string.Empty;

    public string OfficialEmail { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime JoiningDate { get; set; }

    public string EmploymentStatus { get; set; } = "Active";

    public string Extension { get; set; } = string.Empty;

    public string CellPhoneNumber { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    public int? DesignationId { get; set; }
}