using System.Security.Claims;

namespace Hospital_Management_System.Services.StaffManagement;

public static class ClaimsPrincipalExtensions
{
    public static string GetRequiredRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role)
               ?? throw new UnauthorizedAccessException("The authenticated user does not have a role claim.");
    }

    public static int GetRequiredDomainUserId(this ClaimsPrincipal user)
    {
        var claimValue = user.FindFirstValue(DomainUserClaimsTransformation.DomainUserIdClaimType);
        if (int.TryParse(claimValue, out var domainUserId))
        {
            return domainUserId;
        }

        throw new UnauthorizedAccessException(
            "The authenticated user is not linked to a staff profile in the clinic database.");
    }

    public static string GetRequiredActorPublicId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(DomainUserClaimsTransformation.PublicIdClaimType)
               ?? throw new UnauthorizedAccessException(
                   "The authenticated user is not linked to a staff profile in the clinic database.");
    }
}
