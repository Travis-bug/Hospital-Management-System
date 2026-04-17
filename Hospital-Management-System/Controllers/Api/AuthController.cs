using Hospital_Management_System.Data;
using Hospital_Management_System.Models.ViewModels;
using Hospital_Management_System.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    ClinicContext clinicContext,
    IEmailSender emailSender,
    ILogger<AuthController> logger,
    IWebHostEnvironment environment) : ControllerBase
{
    private static readonly string[] RolePriority =
    [
        "Manager",
        "Admin",
        "Doctor",
        "Nurse",
        "Secretary"
    ];

    // The SPA never sees the cookie itself. It only receives enough session data
    // to render the correct workspace after ASP.NET Identity sets the HTTP-only cookie.
    [HttpGet("me")]
    [Authorize]   
    public async Task<ActionResult<AuthSessionDto>> GetCurrentSession()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(new { message = "No active session." });
        }

        return Ok(await BuildSessionAsync(user));
    }

    
    
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<AuthSessionDto>> Login([FromBody] LoginRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        
        var user = await userManager.FindByEmailAsync(dto.Email.Trim());
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid login attempt." });
        } 
        

        
        if (!user.EmailConfirmed)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Email address must be verified before signing in."
            });
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            var emailCode = await userManager.GenerateTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider);

            await emailSender.SendEmailAsync(
                user.Email!,
                "Your Hospital Management System sign-in code",
                BuildTwoFactorEmailHtml(user.Email!, emailCode));

            if (environment.IsDevelopment())
            {
                logger.LogInformation("Development email 2FA code for {Email}: {Code}", user.Email, emailCode);
            }

            return Ok(new AuthSessionDto(
                RequiresTwoFactor: true,
                Role: null,
                Email: user.Email,
                PublicId: null,
                DisplayName: null));
        }

        if (result.Succeeded)
        {
            var session = await BuildSessionAsync(user);
            return Ok(session with { RequiresTwoFactor = false });
        }

        if (result.IsLockedOut)
        {
            return Unauthorized(new { message = "This account is temporarily locked. Try again later." });
        }

        if (result.IsNotAllowed)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Email address must be verified before signing in."
            });
        }

        return Unauthorized(new { message = "Invalid login attempt." });
    }

    [HttpPost("login-2fa")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<AuthSessionDto>> LoginWithTwoFactor([FromBody] TwoFactorLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code))
        {
            return BadRequest(new { message = "Email and authentication code are required." });
        }

        var pendingUser = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (pendingUser is null)
        {
            return Unauthorized(new { message = "Two-factor session expired. Start the login flow again." });
        }

        if (!string.Equals(pendingUser.Email, dto.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = "Two-factor session does not match the requested account." });
        }

        var normalizedCode = dto.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await signInManager.TwoFactorSignInAsync(
            TokenOptions.DefaultEmailProvider,
            normalizedCode,
            isPersistent: false,
            rememberClient: dto.RememberMachine);

        if (result.Succeeded)
        {
            var session = await BuildSessionAsync(pendingUser);
            return Ok(session with { RequiresTwoFactor = false });
        }

        if (result.IsLockedOut)
        {
            return Unauthorized(new { message = "This account is temporarily locked. Try again later." });
        }

        return Unauthorized(new { message = "Invalid 2FA code." });
    }

    [HttpPost("complete-onboarding")]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)
            || string.IsNullOrWhiteSpace(dto.EmailConfirmationToken)
            || string.IsNullOrWhiteSpace(dto.PasswordResetToken)
            || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(new { message = "Email, tokens, and the new password are required." });
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return BadRequest(new { message = "Activation link is invalid or has expired." });
        }

        string emailConfirmationToken;
        string passwordResetToken;

        try
        {
            emailConfirmationToken = IdentityTokenCodec.Decode(dto.EmailConfirmationToken);
            passwordResetToken = IdentityTokenCodec.Decode(dto.PasswordResetToken);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Activation link is invalid or malformed." });
        }

        if (!user.EmailConfirmed)
        {
            var confirmResult = await userManager.ConfirmEmailAsync(user, emailConfirmationToken);
            if (!confirmResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = string.Join(" ", confirmResult.Errors.Select(error => error.Description))
                });
            }
        }

        var resetResult = await userManager.ResetPasswordAsync(user, passwordResetToken, dto.NewPassword);
        if (!resetResult.Succeeded)
        {
            return BadRequest(new
            {
                message = string.Join(" ", resetResult.Errors.Select(error => error.Description))
            });
        }

        await userManager.UpdateSecurityStampAsync(user);

        logger.LogInformation("Completed onboarding for {Email}.", user.Email);

        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(new { message = "Current and new passwords are required." });
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(new { message = "No active session." });
        }

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = string.Join(" ", result.Errors.Select(error => error.Description))
            });
        }

        await signInManager.RefreshSignInAsync(user);
        return NoContent();
    }

    private async Task<AuthSessionDto> BuildSessionAsync(IdentityUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var highestRole = ResolveHighestRole(roles);
        var actor = await ResolveActorAsync(highestRole, user.Id);

        return new AuthSessionDto(
            RequiresTwoFactor: false,
            Role: highestRole,
            Email: user.Email,
            PublicId: actor?.PublicId,
            DisplayName: actor?.DisplayName ?? user.Email);
    }

    private static string? ResolveHighestRole(IEnumerable<string> roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var role in RolePriority)
        {
            if (roleSet.Contains(role))
            {
                return role;
            }
        }

        return roles.FirstOrDefault();
    }

    private async Task<ActorSessionInfo?> ResolveActorAsync(string? role, string identityUserId)
    {
        return role switch
        {
            "Doctor" => await clinicContext.Doctors
                .AsNoTracking()
                .Where(doctor => doctor.IdentityUserId == identityUserId)
                .Select(doctor => new ActorSessionInfo(
                    doctor.PublicId,
                    $"{doctor.FirstName} {doctor.LastName}"))
                .SingleOrDefaultAsync(),

            "Nurse" => await clinicContext.Nurses
                .AsNoTracking()
                .Where(nurse => nurse.IdentityUserId == identityUserId)
                .Select(nurse => new ActorSessionInfo(
                    nurse.PublicId,
                    $"{nurse.FirstName} {nurse.LastName}"))
                .SingleOrDefaultAsync(),

            "Secretary" => await clinicContext.Secretaries
                .AsNoTracking()
                .Where(secretary => secretary.IdentityUserId == identityUserId)
                .Select(secretary => new ActorSessionInfo(
                    secretary.PublicId,
                    $"{secretary.FirstName} {secretary.LastName}"))
                .SingleOrDefaultAsync(),

            "Admin" => await clinicContext.AdministrativeAssistants
                .AsNoTracking()
                .Where(admin => admin.IdentityUserId == identityUserId)
                .Select(admin => new ActorSessionInfo(
                    admin.PublicId,
                    $"{admin.FirstName} {admin.LastName}"))
                .SingleOrDefaultAsync(),

            "Manager" => await clinicContext.Managers
                .AsNoTracking()
                .Where(manager => manager.IdentityUserId == identityUserId)
                .Select(manager => new ActorSessionInfo(
                    manager.PublicId,
                    $"{manager.FirstName} {manager.LastName}"))
                .SingleOrDefaultAsync(),

            _ => null
        };
    }

    private static string BuildTwoFactorEmailHtml(string email, string code)
    {
        return $"""
            <div style="font-family: Arial, sans-serif; color: #0f172a;">
              <h2 style="margin-bottom: 12px;">Hospital Management System sign-in verification</h2>
              <p>A sign-in attempt was made for <strong>{email}</strong>.</p>
              <p>Use the following verification code to complete the login:</p>
              <div style="display: inline-block; margin: 16px 0; padding: 12px 18px; border-radius: 16px; background: #0f172a; color: white; font-size: 24px; font-weight: 700; letter-spacing: 4px;">
                {code}
              </div>
              <p style="margin-top: 16px;">If you did not request this code, you can ignore this email.</p>
            </div>
            """;
    }

    private sealed record ActorSessionInfo(string PublicId, string DisplayName);
}
