using System;
using Hospital_Management_System.Data;
using Hospital_Management_System.Services.ClinicalRecording;
using Hospital_Management_System.Services.PatientManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
 var clinicCs = builder.Configuration.GetConnectionString("ClinicDb")
             ?? throw new InvalidOperationException("Connection string 'ClinicDb' not found.");

 builder.Services.AddDbContext<ClinicContext>(options =>
     options.UseMySql(clinicCs, ServerVersion.AutoDetect(clinicCs)));

// Application services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<ITestResultsService, TestResultService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IPatientService, PatientService>();

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