using System.Globalization;
using System.Security.Claims;
using Hospital_Management_System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Services.StaffManagement;

public sealed class DomainUserClaimsTransformation(ClinicContext clinicContext) : IClaimsTransformation
{
    public const string DomainUserIdClaimType = "DomainUserId";
    public const string PublicIdClaimType = "PublicId";

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return principal;
        }

        if (principal.HasClaim(claim => claim.Type == DomainUserIdClaimType)
            && principal.HasClaim(claim => claim.Type == PublicIdClaimType))
        {
            return principal;
        }

        var identityUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = principal.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(identityUserId) || string.IsNullOrWhiteSpace(role))
        {
            return principal;
        }

        var domainUser = await ResolveDomainUserAsync(role, identityUserId);
        if (domainUser is null)
        {
            return principal;
        }

        if (!principal.HasClaim(claim => claim.Type == DomainUserIdClaimType))
        {
            identity.AddClaim(new Claim(
                DomainUserIdClaimType,
                domainUser.DomainUserId.ToString(CultureInfo.InvariantCulture)));
        }

        if (!principal.HasClaim(claim => claim.Type == PublicIdClaimType))
        {
            identity.AddClaim(new Claim(PublicIdClaimType, domainUser.PublicId));
        }

        return principal;
    }

    private async Task<DomainUserClaimValue?> ResolveDomainUserAsync(string role, string identityUserId)
    {
        return role switch
        {
            "Doctor" => await clinicContext.Doctors
                .AsNoTracking()
                .Where(doctor => doctor.IdentityUserId == identityUserId)
                .Select(doctor => new DomainUserClaimValue(doctor.DoctorId, doctor.PublicId))
                .SingleOrDefaultAsync(),

            "Nurse" => await clinicContext.Nurses
                .AsNoTracking()
                .Where(nurse => nurse.IdentityUserId == identityUserId)
                .Select(nurse => new DomainUserClaimValue(nurse.NurseId, nurse.PublicId))
                .SingleOrDefaultAsync(),

            "Secretary" => await clinicContext.Secretaries
                .AsNoTracking()
                .Where(secretary => secretary.IdentityUserId == identityUserId)
                .Select(secretary => new DomainUserClaimValue(secretary.SecretaryId, secretary.PublicId))
                .SingleOrDefaultAsync(),

            "Admin" => await clinicContext.AdministrativeAssistants
                .AsNoTracking()
                .Where(admin => admin.IdentityUserId == identityUserId)
                .Select(admin => new DomainUserClaimValue(admin.AdminId, admin.PublicId))
                .SingleOrDefaultAsync(),

            "Manager" => await clinicContext.Managers
                .AsNoTracking()
                .Where(manager => manager.IdentityUserId == identityUserId)
                .Select(manager => new DomainUserClaimValue(manager.ManagerId, manager.PublicId))
                .SingleOrDefaultAsync(),

            _ => null
        };
    }

    private sealed record DomainUserClaimValue(int DomainUserId, string PublicId);
}
