using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
// Make sure to add your using statements for your models and context!
using Hospital_Management_System.Models; 

namespace Hospital_Management_System.Data;

public static class StaffIdentityPatcher
{
    public static async Task SyncStaffToIdentityAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var clinicDb = serviceProvider.GetRequiredService<ClinicContext>();

        // 1. Ensure all Roles exist first
        string[] roles = { "Manager", "Admin", "Doctor", "Nurse", "Secretary" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var defaultPassword = config["SeedSettings:DefaultStaffPassword"];

        if (string.IsNullOrEmpty(defaultPassword))
        {
            throw new Exception("CRITICAL: DefaultStaffPassword is missing from appsettings.json!");
        }
        
        

        // 2. Patch Doctors
        var doctors = await clinicDb.Doctors.Where(d => string.IsNullOrEmpty(d.IdentityUserId)).ToListAsync();
        foreach (var doc in doctors)
        {
            var email = $"{doc.FirstName.ToLower()}.{doc.LastName.ToLower()}@hospital.com";
            await CreateAndLinkUser(userManager, doc, email, defaultPassword, "Doctor");
        }

        // 3. Patch Nurses
        var nurses = await clinicDb.Nurses.Where(n => string.IsNullOrEmpty(n.IdentityUserId)).ToListAsync();
        foreach (var nurse in nurses)
        {
            var email = $"{nurse.FirstName.ToLower()}.{nurse.LastName.ToLower()}@hospital.com";
            await CreateAndLinkUser(userManager, nurse, email, defaultPassword, "Nurse");
        }

        // 4. Patch Managers
        var managers = await clinicDb.Managers.Where(m => string.IsNullOrEmpty(m.IdentityUserId)).ToListAsync();
        foreach (var manager in managers)
        {
            var email = $"{manager.FirstName.ToLower()}.{manager.LastName.ToLower()}@hospital.com";
            await CreateAndLinkUser(userManager, manager, email, defaultPassword, "Manager");
        }

        // 5. Patch Admins (Administrative Assistants)
        var admins = await clinicDb.AdministrativeAssistants.Where(a => string.IsNullOrEmpty(a.IdentityUserId)).ToListAsync();
        foreach (var admin in admins)
        {
            var email = $"{admin.FirstName.ToLower()}.{admin.LastName.ToLower()}@hospital.com";
            await CreateAndLinkUser(userManager, admin, email, defaultPassword, "Admin");
        }

        // Save the links to the clinic database!
        await clinicDb.SaveChangesAsync();
    }

    
    // Main Method used to create and link User_identity to Staffs
    private static async Task CreateAndLinkUser(UserManager<IdentityUser> userManager, dynamic staffMember, string email, string password, string role)
    {
        // Check if users already exist in the auth DB, just in case
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser == null)
        {
            var newUser = new IdentityUser { UserName = email, Email = email };
            var result = await userManager.CreateAsync(newUser, password);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, role);
                staffMember.IdentityUserId = newUser.Id; // Link them!
                Console.WriteLine($"[PATCHER] Created {role} login: {email}");
            }
        }
        else
        {
            staffMember.IdentityUserId = existingUser.Id; // Link them if they somehow existed
        }
    }
}