using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Management_System.Data;

public static class StaffIdentityPatcher
{
    private static readonly string[] SupportedRoles =
    [
        "Manager",
        "Admin",
        "Doctor",
        "Nurse",
        "Secretary"
    ];

    public static async Task SyncStaffToIdentityAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var clinicDb = serviceProvider.GetRequiredService<ClinicContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var defaultPassword = configuration["SeedSettings:DefaultStaffPassword"];
        if (string.IsNullOrWhiteSpace(defaultPassword))
        {
            throw new InvalidOperationException("SeedSettings:DefaultStaffPassword is missing.");
        }

        var emailDomain = configuration["SeedSettings:DemoIdentityEmailDomain"] ?? "hospital.com";

        await EnsureRolesAsync(roleManager);

        await SyncRoleAsync(
            clinicDb.Doctors,
            "Doctor",
            staff => staff.PublicId,
            staff => staff.FirstName,
            staff => staff.LastName,
            staff => staff.IdentityUserId,
            (staff, identityUserId) => staff.IdentityUserId = identityUserId,
            userManager,
            passwordHasher,
            logger,
            defaultPassword,
            emailDomain);

        await SyncRoleAsync(
            clinicDb.Nurses,
            "Nurse",
            staff => staff.PublicId,
            staff => staff.FirstName,
            staff => staff.LastName,
            staff => staff.IdentityUserId,
            (staff, identityUserId) => staff.IdentityUserId = identityUserId,
            userManager,
            passwordHasher,
            logger,
            defaultPassword,
            emailDomain);

        await SyncRoleAsync(
            clinicDb.Secretaries,
            "Secretary",
            staff => staff.PublicId,
            staff => staff.FirstName,
            staff => staff.LastName,
            staff => staff.IdentityUserId,
            (staff, identityUserId) => staff.IdentityUserId = identityUserId,
            userManager,
            passwordHasher,
            logger,
            defaultPassword,
            emailDomain);

        await SyncRoleAsync(
            clinicDb.AdministrativeAssistants,
            "Admin",
            staff => staff.PublicId,
            staff => staff.FirstName,
            staff => staff.LastName,
            staff => staff.IdentityUserId,
            (staff, identityUserId) => staff.IdentityUserId = identityUserId,
            userManager,
            passwordHasher,
            logger,
            defaultPassword,
            emailDomain);

        await SyncRoleAsync(
            clinicDb.Managers,
            "Manager",
            staff => staff.PublicId,
            staff => staff.FirstName,
            staff => staff.LastName,
            staff => staff.IdentityUserId,
            (staff, identityUserId) => staff.IdentityUserId = identityUserId,
            userManager,
            passwordHasher,
            logger,
            defaultPassword,
            emailDomain);

        await clinicDb.SaveChangesAsync();
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in SupportedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SyncRoleAsync<TStaff>(
        DbSet<TStaff> staffSet,
        string role,
        Func<TStaff, string> publicIdSelector,
        Func<TStaff, string> firstNameSelector,
        Func<TStaff, string> lastNameSelector,
        Func<TStaff, string?> identityUserIdSelector,
        Action<TStaff, string> identityUserIdSetter,
        UserManager<IdentityUser> userManager,
        IPasswordHasher<IdentityUser> passwordHasher,
        ILogger logger,
        string defaultPassword,
        string emailDomain)
        where TStaff : class
    {
        var staffMembers = await staffSet.ToListAsync();

        foreach (var staffMember in staffMembers)
        {
            var publicId = publicIdSelector(staffMember);
            var expectedEmail = BuildSeedEmail(role, publicId, emailDomain);
            var createdNewUser = false;

            IdentityUser? user = null;
            var linkedIdentityUserId = identityUserIdSelector(staffMember);
            if (!string.IsNullOrWhiteSpace(linkedIdentityUserId))
            {
                user = await userManager.FindByIdAsync(linkedIdentityUserId);
                if (user == null)
                {
                    logger.LogWarning(
                        "[PATCHER] {Role} staff {PublicId} referenced missing identity user {IdentityUserId}. Rebuilding login.",
                        role,
                        publicId,
                        linkedIdentityUserId);
                }
            }

            user ??= await userManager.FindByEmailAsync(expectedEmail);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = expectedEmail,
                    Email = expectedEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };

                var createResult = await userManager.CreateAsync(user, defaultPassword);
                if (!createResult.Succeeded)
                {
                    logger.LogError(
                        "[PATCHER] Failed to create {Role} login for {PublicId}: {Errors}",
                        role,
                        publicId,
                        string.Join(", ", createResult.Errors.Select(error => error.Description)));
                    continue;
                }

                createdNewUser = true;

                logger.LogInformation(
                    "[PATCHER] Created {Role} login {Email} for staff {PublicId}.",
                    role,
                    expectedEmail,
                    publicId);
            }

            await NormalizeIdentityAsync(
                userManager,
                passwordHasher,
                user,
                role,
                expectedEmail,
                defaultPassword,
                createdNewUser);

            identityUserIdSetter(staffMember, user.Id);

            logger.LogInformation(
                "[PATCHER] Synced {Role} staff {PublicId} ({FirstName} {LastName}) -> {Email}.",
                role,
                publicId,
                firstNameSelector(staffMember),
                lastNameSelector(staffMember),
                expectedEmail);
        }
    }

    private static string BuildSeedEmail(string role, string publicId, string emailDomain)
    {
        return $"{role.ToLowerInvariant()}.{publicId.ToLowerInvariant()}@{emailDomain}";
    }

    private static async Task NormalizeIdentityAsync(
        UserManager<IdentityUser> userManager,
        IPasswordHasher<IdentityUser> passwordHasher,
        IdentityUser user,
        string requiredRole,
        string expectedEmail,
        string defaultPassword,
        bool createdNewUser)
    {
        // Demo users created from clinic-side seed data get deterministic emails so
        // the dataset is easy to reason about. Existing linked users are not renamed
        // or re-passworded on every restart because that would override real staff
        // provisioning state.
        if (createdNewUser)
        {
            user.UserName = expectedEmail;
            user.Email = expectedEmail;
            user.PasswordHash = passwordHasher.HashPassword(user, defaultPassword);
            user.SecurityStamp = Guid.NewGuid().ToString("N");
        }

        user.EmailConfirmed = true;
        user.LockoutEnabled = true;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Unable to update the identity record for {expectedEmail}: {string.Join(", ", updateResult.Errors.Select(error => error.Description))}");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        foreach (var role in currentRoles.Where(role => !string.Equals(role, requiredRole, StringComparison.OrdinalIgnoreCase)))
        {
            await userManager.RemoveFromRoleAsync(user, role);
        }

        if (!await userManager.IsInRoleAsync(user, requiredRole))
        {
            await userManager.AddToRoleAsync(user, requiredRole);
        }

        if (createdNewUser)
        {
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
        }
    }
}
