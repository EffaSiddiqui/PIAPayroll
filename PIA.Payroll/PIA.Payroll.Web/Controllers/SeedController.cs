using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA.Payroll.Domain.Entities;
using PIA.Payroll.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace PIA.Payroll.Web.Controllers;

[AllowAnonymous]
[Route("[controller]")]
public class SeedController : Controller
{
    private readonly PayrollDbContext _context;

    public SeedController(PayrollDbContext context)
    {
        _context = context;
    }

    [HttpGet("Run")]
    public async Task<IActionResult> Run()
    {
        var rawData = new[]
        {
            new { Name = "MUHAMMAD ASIM", PNO = "635673", Desig = "DEPUTY GENERAL MANAGER", Ext = "45020,0,0", Cell = "", Email = "m.asim@piac.aero", Loc = "ERP-HALL-G-FLOOR.H/O" },
            new { Name = "ASAD HASNAIN BUKHARI", PNO = "59775", Desig = "GENERAL MANAGER", Ext = "", Cell = "", Email = "pmo.erp@piac.aero", Loc = "GM E.R.P, G/FLOOR,H/O.KARACHI" },
            new { Name = "MUHAMMAD MUSTUFA", PNO = "67587", Desig = "", Ext = "45020,0", Cell = "", Email = "", Loc = "" },
            new { Name = "NOBY FRANCIS", PNO = "XXXXX1", Desig = "HR", Ext = "", Cell = "0300-2704623", Email = "noby.francis@piac.aero", Loc = "" },
            new { Name = "RAFAY ASHFAQ ANSARI", PNO = "65321", Desig = "SUPERVISOR", Ext = "45029,0,0", Cell = "", Email = "rafay.ansari@piac.aero", Loc = "ERP HALL HEAD OFFICE" },
            new { Name = "MUHAMMAD ABID ALI", PNO = "58509", Desig = "INCHARGE", Ext = "44713,0,0", Cell = "", Email = "", Loc = "ERP HALL HEAD OFFICE" },
            new { Name = "SIDRA MOHSIN", PNO = "61550", Desig = "STAFF OFFICER", Ext = "", Cell = "0345-3253127", Email = "sidra.mohsin@piac.aero", Loc = "" },
            new { Name = "WASEEM", PNO = "57044", Desig = "SUPERVISOR", Ext = "47254,0,0", Cell = "", Email = "rsdprocurement@piac.aero", Loc = "STAFF OFFICER,R#2A,ERP,H/O KHI" },
            new { Name = "FARHAN KHAN", PNO = "0", Desig = "MANAGER", Ext = "45021,0,0", Cell = "", Email = "", Loc = "" },
            new { Name = "SHAHBAZ ALI KHAN", PNO = "62445", Desig = "ASSISTANT MANAGER", Ext = "45027,0,0", Cell = "0321-2358997", Email = "shahbaz.khan@piac.aero", Loc = "" },
            new { Name = "SHAZIA", PNO = "XXXXX2", Desig = "OFFICER", Ext = "45028,0,0", Cell = "", Email = "", Loc = "" },
            new { Name = "SOFIA NASEEM", PNO = "48936", Desig = "DEPUTY GENERAL MANAGER", Ext = "43163,43144,0", Cell = "0323-2327401", Email = "sufia.naseem@piac.aero", Loc = "IT (INFORMATION & COMMUNICATION TECHNOLOGY)" },
            new { Name = "NADEEM MAHMOOD KHAN", PNO = "67488", Desig = "DEPUTY GENERAL MANAGER", Ext = "44554,45353,0", Cell = "0322-6766356", Email = "nadeem.mahmood@piac.aero", Loc = "ENTERPRISES RESOURCE PLANNING HALL HEAD OFFICE" },
            new { Name = "ABDUL HAFEEZ", PNO = "64513", Desig = "ASSISTANT MANAGER", Ext = "45034,0,0", Cell = "0323-8556568", Email = "", Loc = "" },
            new { Name = "MANNAN ASGHAR", PNO = "4873", Desig = "SENIOR OFFICER", Ext = "45031,0,0", Cell = "0333-2186505", Email = "", Loc = "" },
            new { Name = "MOHSIN ANWAR", PNO = "67438", Desig = "ASSISTANT MANAGER", Ext = "45017,0,0", Cell = "0345-3050207", Email = "rana.mohsin@piac.aero", Loc = "" },
            new { Name = "MUHAMMAD NAVEED AZHAR", PNO = "60985", Desig = "DEPUTY GENERAL MANAGER", Ext = "45016,45123,0", Cell = "0323-2320542", Email = "naveed.azhar@piac.aero", Loc = "" },
            new { Name = "SYED BASIT FAHEEM", PNO = "60988", Desig = "MANAGER", Ext = "45032,0,0", Cell = "0323-2320550", Email = "basit.faheem@piac.aero", Loc = "" },
            new { Name = "ZAFAR ABBAS", PNO = "4852", Desig = "OFFICER", Ext = "45033,0.0", Cell = "0341-3073687", Email = "", Loc = "" },
            new { Name = "ALI ISMAIL", PNO = "23", Desig = "SENIOR OFFICER", Ext = "450260,0", Cell = "0336-2205445", Email = "", Loc = "" },
            new { Name = "ZUBAIR WAQAR", PNO = "68168", Desig = "MANAGER", Ext = "45113,0,0", Cell = "", Email = "zubair.waqar@piac.aero", Loc = "SCM ERP HALL HEAD OFFICE" },
            new { Name = "MUKHTAR RAZA", PNO = "53036", Desig = "DEPUTY GENERAL MANAGER", Ext = "45018,0,0", Cell = "", Email = "", Loc = "DGM,ENTERPRISES RESOURCE PLANNING HALL HEAD OFFICE" },
            new { Name = "UMAIR OBAID", PNO = "XXXXX3", Desig = "SENIOR OFFICER", Ext = "43849,0,03849", Cell = "0323-2326853", Email = "", Loc = "" },
            new { Name = "YASIR SHEIKH", PNO = "64398", Desig = "ASSISTANT MANAGER", Ext = "45023,0,0", Cell = "0333-4641235", Email = "", Loc = "" },
            new { Name = "MUHAMMAD SAAD", PNO = "68169", Desig = "SR, ORACLE DEVELOPER ICT", Ext = "", Cell = "0322-6766357", Email = "", Loc = "TECHNICAL" }
        };

        var departments = await _context.Departments.ToListAsync();
        var defaultDeptId = departments.FirstOrDefault()?.Id ?? 1;

        foreach (var item in rawData)
        {
            if (await _context.Employees.AnyAsync(e => e.EmployeeCode == item.PNO))
                continue;

            int? desigId = null;
            if (!string.IsNullOrWhiteSpace(item.Desig))
            {
                var desig = await _context.Designations.FirstOrDefaultAsync(d => d.Name == item.Desig);
                if (desig == null)
                {
                    desig = new Designation { Name = item.Desig, Code = item.Desig.Substring(0, Math.Min(3, item.Desig.Length)).ToUpper(), IsActive = true };
                    _context.Designations.Add(desig);
                    await _context.SaveChangesAsync();
                }
                desigId = desig.Id;
            }

            var nameParts = item.Name.Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "Unknown";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            var emp = new Employee
            {
                EmployeeCode = item.PNO,
                FirstName = firstName,
                LastName = lastName,
                FatherName = "-",
                CNIC = "00000-0000000-0",
                DateOfBirth = DateTime.Today.AddYears(-30),
                Gender = "Male",
                OfficialEmail = item.Email,
                Extension = item.Ext,
                CellPhoneNumber = item.Cell,
                Location = item.Loc,
                DepartmentId = defaultDeptId,
                DesignationId = desigId,
                JoiningDate = DateTime.Today,
                EmploymentStatus = "Active"
            };

            _context.Employees.Add(emp);
        }

        await _context.SaveChangesAsync();
        return Content("Seeded");
    }
}
