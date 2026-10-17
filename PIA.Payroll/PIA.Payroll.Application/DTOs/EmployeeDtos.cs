using System.ComponentModel.DataAnnotations;

namespace PIA.Payroll.Application.DTOs;

public class EmployeeListItemDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string CNIC { get; set; } = string.Empty;
    public string OfficialEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DesignationName { get; set; }
    public int? DesignationId { get; set; }
    public DateTime JoiningDate { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public bool IsActive => string.Equals(EmploymentStatus, "Active", StringComparison.OrdinalIgnoreCase);

    public string Extension { get; set; } = string.Empty;
    public string CellPhoneNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class EmployeeDetailsDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string FatherName { get; set; } = string.Empty;
    public string CNIC { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string PersonalEmail { get; set; } = string.Empty;
    public string OfficialEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public bool IsActive => string.Equals(EmploymentStatus, "Active", StringComparison.OrdinalIgnoreCase);

    public string Extension { get; set; } = string.Empty;
    public string CellPhoneNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class EmployeeCreateDto
{
    [Required(ErrorMessage = "Employee Code is required.")]
    [Display(Name = "Employee Code")]
    [StringLength(20, ErrorMessage = "Employee Code cannot exceed 20 characters.")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "First Name is required.")]
    [Display(Name = "First Name")]
    [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name is required.")]
    [Display(Name = "Last Name")]
    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Father's Name is required.")]
    [Display(Name = "Father's Name")]
    [StringLength(100, ErrorMessage = "Father's Name cannot exceed 100 characters.")]
    public string FatherName { get; set; } = string.Empty;

    [Required(ErrorMessage = "CNIC is required.")]
    [Display(Name = "CNIC (National ID)")]
    [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC must follow the standard format: XXXXX-XXXXXXX-X")]
    public string CNIC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-25);

    [Required(ErrorMessage = "Gender is required.")]
    [Display(Name = "Gender")]
    public string Gender { get; set; } = "Male";

    [EmailAddress(ErrorMessage = "Invalid personal email address.")]
    [Display(Name = "Personal Email")]
    public string PersonalEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Official Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid corporate email address.")]
    [Display(Name = "Corporate / Official Email")]
    public string OfficialEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [Display(Name = "Contact Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Extension")]
    public string Extension { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid cell phone number.")]
    [Display(Name = "Cell Phone Number")]
    public string CellPhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Location")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Joining Date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Joining")]
    public DateTime JoiningDate { get; set; } = DateTime.Today;

    [Display(Name = "Initial Status")]
    public string EmploymentStatus { get; set; } = "Active";

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Designation / Role")]
    public int? DesignationId { get; set; }
}

public class EmployeeEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Employee Code is required.")]
    [Display(Name = "Employee Code")]
    [StringLength(20, ErrorMessage = "Employee Code cannot exceed 20 characters.")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "First Name is required.")]
    [Display(Name = "First Name")]
    [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name is required.")]
    [Display(Name = "Last Name")]
    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Father's Name is required.")]
    [Display(Name = "Father's Name")]
    [StringLength(100, ErrorMessage = "Father's Name cannot exceed 100 characters.")]
    public string FatherName { get; set; } = string.Empty;

    [Required(ErrorMessage = "CNIC is required.")]
    [Display(Name = "CNIC (National ID)")]
    [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC must follow the standard format: XXXXX-XXXXXXX-X")]
    public string CNIC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required.")]
    [Display(Name = "Gender")]
    public string Gender { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid personal email address.")]
    [Display(Name = "Personal Email")]
    public string PersonalEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Official Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid corporate email address.")]
    [Display(Name = "Corporate / Official Email")]
    public string OfficialEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [Display(Name = "Contact Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Extension")]
    public string Extension { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid cell phone number.")]
    [Display(Name = "Cell Phone Number")]
    public string CellPhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Location")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Joining Date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Joining")]
    public DateTime JoiningDate { get; set; }

    [Required(ErrorMessage = "Employment Status is required.")]
    [Display(Name = "Employment Status")]
    public string EmploymentStatus { get; set; } = "Active";

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Designation / Role")]
    public int? DesignationId { get; set; }
}

public class EmployeeFilterDto
{
    public string? SearchTerm { get; set; }
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public string? Status { get; set; } = "All";
}

public class LookupItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
