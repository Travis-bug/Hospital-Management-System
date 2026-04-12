using Hospital_Management_System.Data;
using Hospital_Management_System.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager,Admin")]
public class StaffController(
    ClinicContext clinicContext,
    AppIdentityDbContext identityContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffDirectoryItemDto>>> GetStaff()
    {
        var emailByIdentityId = await identityContext.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.Email);

        var directory = new List<StaffDirectoryItemDto>();

        var doctors = await clinicContext.Doctors
            .AsNoTracking()
            .ToListAsync();
        directory.AddRange(doctors.Select(doctor => new StaffDirectoryItemDto(
                doctor.PublicId,
                doctor.FirstName,
                doctor.LastName,
                "Doctor",
                doctor.IdentityUserId != null && emailByIdentityId.ContainsKey(doctor.IdentityUserId)
                    ? emailByIdentityId[doctor.IdentityUserId]
                    : null,
                string.IsNullOrWhiteSpace(doctor.Specialization) ? "General Practice" : doctor.Specialization,
                string.IsNullOrWhiteSpace(doctor.IdentityUserId) ? "Provisioned" : "Active")));

        var nurses = await clinicContext.Nurses
            .AsNoTracking()
            .ToListAsync();
        directory.AddRange(nurses.Select(nurse => new StaffDirectoryItemDto(
                nurse.PublicId,
                nurse.FirstName,
                nurse.LastName,
                "Nurse",
                nurse.IdentityUserId != null && emailByIdentityId.ContainsKey(nurse.IdentityUserId)
                    ? emailByIdentityId[nurse.IdentityUserId]
                    : null,
                "Nursing",
                string.IsNullOrWhiteSpace(nurse.IdentityUserId) ? "Provisioned" : "Active")));

        var secretaries = await clinicContext.Secretaries
            .AsNoTracking()
            .ToListAsync();
        directory.AddRange(secretaries.Select(secretary => new StaffDirectoryItemDto(
                secretary.PublicId,
                secretary.FirstName,
                secretary.LastName,
                "Secretary",
                secretary.IdentityUserId != null && emailByIdentityId.ContainsKey(secretary.IdentityUserId)
                    ? emailByIdentityId[secretary.IdentityUserId]
                    : null,
                "Front Desk",
                string.IsNullOrWhiteSpace(secretary.IdentityUserId) ? "Provisioned" : "Active")));

        var admins = await clinicContext.AdministrativeAssistants
            .AsNoTracking()
            .ToListAsync();
        directory.AddRange(admins.Select(admin => new StaffDirectoryItemDto(
                admin.PublicId,
                admin.FirstName,
                admin.LastName,
                "Admin",
                admin.IdentityUserId != null && emailByIdentityId.ContainsKey(admin.IdentityUserId)
                    ? emailByIdentityId[admin.IdentityUserId]
                    : null,
                "Operations",
                string.IsNullOrWhiteSpace(admin.IdentityUserId) ? "Provisioned" : "Active")));

        var managers = await clinicContext.Managers
            .AsNoTracking()
            .ToListAsync();
        directory.AddRange(managers.Select(manager => new StaffDirectoryItemDto(
                manager.PublicId,
                manager.FirstName,
                manager.LastName,
                "Manager",
                manager.IdentityUserId != null && emailByIdentityId.ContainsKey(manager.IdentityUserId)
                    ? emailByIdentityId[manager.IdentityUserId]
                    : null,
                "Management",
                string.IsNullOrWhiteSpace(manager.IdentityUserId) ? "Provisioned" : "Active")));

        return Ok(directory
            .OrderBy(staffMember => staffMember.Role)
            .ThenBy(staffMember => staffMember.LastName)
            .ThenBy(staffMember => staffMember.FirstName));
    }
}
