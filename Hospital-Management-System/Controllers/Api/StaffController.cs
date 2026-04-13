using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.StaffManagement;
using Hospital_Management_System.Utilities;
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
    AppIdentityDbContext identityContext,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffDirectoryItemDto>>> GetStaff()
    {
        var emailByIdentityId = await identityContext.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.Email);

        var directory = new List<StaffDirectoryItemDto>();

        var doctors = await clinicContext.Doctors.AsNoTracking().ToListAsync();
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

        var nurses = await clinicContext.Nurses.AsNoTracking().ToListAsync();
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

        var secretaries = await clinicContext.Secretaries.AsNoTracking().ToListAsync();
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

        var admins = await clinicContext.AdministrativeAssistants.AsNoTracking().ToListAsync();
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

        var managers = await clinicContext.Managers.AsNoTracking().ToListAsync();
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

    [HttpPost]
    public async Task<ActionResult<ProvisionedStaffAccountDto>> CreateStaff([FromBody] CreateStaffAccountDto dto)
    {
        var actorRole = User.GetRequiredRole();
        var allowedRoles = actorRole == "Manager"
            ? new[] { "Doctor", "Nurse", "Secretary", "Admin", "Manager" }
            : new[] { "Doctor", "Nurse", "Secretary" };
        var requestedRole = allowedRoles.FirstOrDefault(role =>
            string.Equals(role, dto.Role, StringComparison.OrdinalIgnoreCase));

        if (requestedRole == null)
        {
            return Forbid();
        }

        if (!await roleManager.RoleExistsAsync(requestedRole))
        {
            await roleManager.CreateAsync(new IdentityRole(requestedRole));
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            return Conflict(new { message = "An identity account with that email already exists." });
        }

        var identityUser = new IdentityUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var createResult = await userManager.CreateAsync(identityUser, dto.TemporaryPassword);
        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                message = string.Join(" ", createResult.Errors.Select(error => error.Description))
            });
        }

        await userManager.AddToRoleAsync(identityUser, requestedRole);

        try
        {
            var publicId = requestedRole switch
            {
                "Doctor" => await CreateDoctorAsync(dto, identityUser.Id),
                "Nurse" => await CreateNurseAsync(dto, identityUser.Id),
                "Secretary" => await CreateSecretaryAsync(dto, identityUser.Id),
                "Admin" => await CreateAdminAsync(dto, identityUser.Id),
                "Manager" => await CreateManagerAsync(dto, identityUser.Id),
                _ => throw new InvalidOperationException("Unsupported role.")
            };

            await clinicContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStaff), null, new ProvisionedStaffAccountDto(
                publicId,
                requestedRole,
                normalizedEmail,
                $"{dto.FirstName.Trim()} {dto.LastName.Trim()}"));
        }
        catch
        {
            await userManager.DeleteAsync(identityUser);
            throw;
        }
    }

    private async Task<string> CreateDoctorAsync(CreateStaffAccountDto dto, string identityUserId)
    {
        var doctor = new Doctor
        {
            PublicId = SecureIdGenerator.GenerateID(10, "DR"),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Specialization = "General Practice",
            IdentityUserId = identityUserId,
            IsTriageQualified = false
        };

        await clinicContext.Doctors.AddAsync(doctor);
        return doctor.PublicId;
    }

    private async Task<string> CreateNurseAsync(CreateStaffAccountDto dto, string identityUserId)
    {
        var nurse = new Nurse
        {
            PublicId = SecureIdGenerator.GenerateID(10, "NR"),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            HourlyRate = 35.00m,
            IdentityUserId = identityUserId
        };

        await clinicContext.Nurses.AddAsync(nurse);
        return nurse.PublicId;
    }

    private async Task<string> CreateSecretaryAsync(CreateStaffAccountDto dto, string identityUserId)
    {
        var secretary = new Secretary
        {
            PublicId = SecureIdGenerator.GenerateID(10, "SC"),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            HourlyRate = 25.00m,
            IdentityUserId = identityUserId
        };

        await clinicContext.Secretaries.AddAsync(secretary);
        return secretary.PublicId;
    }

    private async Task<string> CreateAdminAsync(CreateStaffAccountDto dto, string identityUserId)
    {
        var admin = new AdministrativeAssistant
        {
            PublicId = SecureIdGenerator.GenerateID(10, "Ad"),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            HourlyRate = 20.00m,
            IdentityUserId = identityUserId
        };

        await clinicContext.AdministrativeAssistants.AddAsync(admin);
        return admin.PublicId;
    }

    private async Task<string> CreateManagerAsync(CreateStaffAccountDto dto, string identityUserId)
    {
        var manager = new Manager
        {
            PublicId = SecureIdGenerator.GenerateID(9, "MA"),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            HourlyRate = 42.00m,
            IdentityUserId = identityUserId
        };

        await clinicContext.Managers.AddAsync(manager);
        return manager.PublicId;
    }
}
