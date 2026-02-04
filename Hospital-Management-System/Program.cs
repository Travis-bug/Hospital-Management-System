using Clinic_Management.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Identity UI endpoints
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity DB (clinic_auth)
var authCs = builder.Configuration.GetConnectionString("AuthDb")
             ?? throw new InvalidOperationException("Connection string 'AuthDb' not found.");

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseMySql(authCs, ServerVersion.AutoDetect(authCs)));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // flip true later if you want
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppIdentityDbContext>();

// Clinic DB (Group37Schema) -> add after scaffolding ClinicContext
// var clinicCs = builder.Configuration.GetConnectionString("ClinicDb")
//              ?? throw new InvalidOperationException("Connection string 'ClinicDb' not found.");
// builder.Services.AddDbContext<ClinicContext>(options =>
//     options.UseMySql(clinicCs, ServerVersion.AutoDetect(clinicCs)));

var app = builder.Build();

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // keep if you like nice dev DB error pages
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // <-- IMPORTANT
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity UI

app.Run();