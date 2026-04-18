using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Services.StaffManagement;
using Hospital_Management_System.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController(
    IConfiguration configuration,
    ClinicContext clinicContext,
    AppIdentityDbContext identityContext,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IEmailSender emailSender,
    ILogger<StaffController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Manager,Admin,Secretary")]
    public async Task<ActionResult<IEnumerable<StaffDirectoryItemDto>>> GetStaff()
    {
        var identityInfoById = await identityContext.Users
            .AsNoTracking()
            .ToDictionaryAsync(
                user => user.Id,
                user => new IdentityAccountSnapshot(user.Email, user.EmailConfirmed));

        var directory = new List<StaffDirectoryItemDto>();

        var doctors = await clinicContext.Doctors.AsNoTracking().ToListAsync();
        directory.AddRange(doctors.Select(doctor => new StaffDirectoryItemDto(
            doctor.PublicId,
            doctor.FirstName,
            doctor.LastName,
            "Doctor",
            doctor.IdentityUserId != null && identityInfoById.ContainsKey(doctor.IdentityUserId)
                ? identityInfoById[doctor.IdentityUserId].Email
                : null,
            string.IsNullOrWhiteSpace(doctor.Specialization) ? "General Practice" : doctor.Specialization,
            ResolveAccountStatus(doctor.IdentityUserId, identityInfoById))));

        var nurses = await clinicContext.Nurses.AsNoTracking().ToListAsync();
        directory.AddRange(nurses.Select(nurse => new StaffDirectoryItemDto(
            nurse.PublicId,
            nurse.FirstName,
            nurse.LastName,
            "Nurse",
            nurse.IdentityUserId != null && identityInfoById.ContainsKey(nurse.IdentityUserId)
                ? identityInfoById[nurse.IdentityUserId].Email
                : null,
            "Nursing",
            ResolveAccountStatus(nurse.IdentityUserId, identityInfoById))));

        var secretaries = await clinicContext.Secretaries.AsNoTracking().ToListAsync();
        directory.AddRange(secretaries.Select(secretary => new StaffDirectoryItemDto(
            secretary.PublicId,
            secretary.FirstName,
            secretary.LastName,
            "Secretary",
            secretary.IdentityUserId != null && identityInfoById.ContainsKey(secretary.IdentityUserId)
                ? identityInfoById[secretary.IdentityUserId].Email
                : null,
            "Front Desk",
            ResolveAccountStatus(secretary.IdentityUserId, identityInfoById))));

        var admins = await clinicContext.AdministrativeAssistants.AsNoTracking().ToListAsync();
        directory.AddRange(admins.Select(admin => new StaffDirectoryItemDto(
            admin.PublicId,
            admin.FirstName,
            admin.LastName,
            "Admin",
            admin.IdentityUserId != null && identityInfoById.ContainsKey(admin.IdentityUserId)
                ? identityInfoById[admin.IdentityUserId].Email
                : null,
            "Operations",
            ResolveAccountStatus(admin.IdentityUserId, identityInfoById))));

        var managers = await clinicContext.Managers.AsNoTracking().ToListAsync();
        directory.AddRange(managers.Select(manager => new StaffDirectoryItemDto(
            manager.PublicId,
            manager.FirstName,
            manager.LastName,
            "Manager",
            manager.IdentityUserId != null && identityInfoById.ContainsKey(manager.IdentityUserId)
                ? identityInfoById[manager.IdentityUserId].Email
                : null,
            "Management",
            ResolveAccountStatus(manager.IdentityUserId, identityInfoById))));

        return Ok(directory
            .OrderBy(staffMember => staffMember.Role)
            .ThenBy(staffMember => staffMember.LastName)
            .ThenBy(staffMember => staffMember.FirstName));
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
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
            EmailConfirmed = false,
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

            var emailConfirmationToken = IdentityTokenCodec.Encode(
                await userManager.GenerateEmailConfirmationTokenAsync(identityUser));
            var passwordResetToken = IdentityTokenCodec.Encode(
                await userManager.GeneratePasswordResetTokenAsync(identityUser));

            var activationBaseUrl = ResolveActivationBaseUrl(configuration, Request);
            var activationUrl =
                $"{activationBaseUrl}/activate-account" +
                $"?email={Uri.EscapeDataString(normalizedEmail)}" +
                $"&emailToken={Uri.EscapeDataString(emailConfirmationToken)}" +
                $"&passwordToken={Uri.EscapeDataString(passwordResetToken)}";

            await emailSender.SendEmailAsync(
                normalizedEmail,
                "Activate your Hospital Management System account",
                BuildActivationEmailHtml(dto.FirstName.Trim(), requestedRole, activationUrl));

            logger.LogInformation(
                "Provisioned {Role} account {Email} and sent activation email.",
                requestedRole,
                normalizedEmail);

            return CreatedAtAction(nameof(GetStaff), null, new ProvisionedStaffAccountDto(
                publicId,
                requestedRole,
                normalizedEmail,
                $"{dto.FirstName.Trim()} {dto.LastName.Trim()}",
                true));
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

    private static string ResolveAccountStatus(
        string? identityUserId,
        IReadOnlyDictionary<string, IdentityAccountSnapshot> identityInfoById)
    {
        if (string.IsNullOrWhiteSpace(identityUserId) || !identityInfoById.TryGetValue(identityUserId, out var account))
        {
            return "Provisioned";
        }

        return account.EmailConfirmed ? "Active" : "Pending Activation";
    }

    
    
    
    
    /// <summary>
    /// This controller receives the base URL and checks if it is empty,
    /// and if not, it trims the trailing "/", then returns the HTTP request scheme along with the host header 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    private static string ResolveActivationBaseUrl(IConfiguration configuration, HttpRequest request) 
    {
        var configuredBaseUrl = configuration["Frontend:PublicBaseUrl"]
            ?? configuration["App:PublicBaseUrl"];

        if (!string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            return configuredBaseUrl.TrimEnd('/');
        }

        return $"{request.Scheme}://{request.Host}";
    }

    
    
    
    
    
    private static string BuildActivationEmailHtml(string firstName, string role, string activationUrl)
    {
        return $"""
<div style="font-family: Arial, sans-serif; color: #0f172a; line-height: 1.6;">
  <p style="font-size: 12px; letter-spacing: 0.18em; text-transform: uppercase; color: #64748b;">Hospital Management System</p>
  <h1 style="font-size: 28px; margin-bottom: 8px;">Activate your staff account</h1>
  <p>Hello {System.Net.WebUtility.HtmlEncode(firstName)},</p>
  <p>An internal {System.Net.WebUtility.HtmlEncode(role)} account has been created for you in the Hospital Management System.</p>
  <p>To activate the account, confirm your email address and set your real password using the secure link below.</p>
  <p>
    <a href="{activationUrl}" style="display: inline-block; padding: 12px 18px; border-radius: 999px; background: #1d4ed8; color: white; text-decoration: none; font-weight: 600;">
      Activate account
    </a>
  </p>
  <p>If the button does not work, open this URL directly:</p>
  <p><a href="{activationUrl}">{activationUrl}</a></p>
</div>
""";
    }
    

    private sealed record IdentityAccountSnapshot(string? Email, bool EmailConfirmed);
}
