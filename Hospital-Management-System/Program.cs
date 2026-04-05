using System;
using System.Threading.RateLimiting;
using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.ClinicalRecording;
using Hospital_Management_System.Services.PatientManagement;
using Hospital_Management_System.Services.Scheduling;
using Hospital_Management_System.Services.StaffManagement;
using Hospital_Management_System.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────
// FRAMEWORK SERVICES
// ─────────────────────────────────────────────────────────────────

// M-04: Structured error responses for API endpoints
builder.Services.AddProblemDetails();

// MVC + Identity UI endpoints
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ─────────────────────────────────────────────────────────────────
// C-02: RATE LIMITING — brute-force protection on auth endpoints
// ─────────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window          = TimeSpan.FromMinutes(5);
        opt.PermitLimit     = 10;   // 10 attempts per 5 min window
        opt.QueueLimit      = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ─────────────────────────────────────────────────────────────────
// M-01: CORS — explicit allowlist, never wildcard
// ─────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("HmsPolicy", policy =>
        policy.WithOrigins("https://localhost:7000") // TODO: replace with production domain
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Authorization", "Content-Type")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ─────────────────────────────────────────────────────────────────
// DATABASE CONTEXTS
// L-02: Pin MySQL version — eliminates an extra round-trip on startup
// ─────────────────────────────────────────────────────────────────
var mySqlVersion = new MySqlServerVersion(new Version(8, 0, 36));

var authCs = builder.Configuration.GetConnectionString("AuthDb")
             ?? throw new InvalidOperationException("Connection string 'AuthDb' not found.");

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseMySql(authCs, mySqlVersion));

var clinicCs = builder.Configuration.GetConnectionString("ClinicDb")
             ?? throw new InvalidOperationException("Connection string 'ClinicDb' not found.");

builder.Services.AddDbContext<ClinicContext>(options =>
    options.UseMySql(clinicCs, mySqlVersion));

// ─────────────────────────────────────────────────────────────────
// H-01 & L-01: IDENTITY — email confirmation + strict lockout
// ─────────────────────────────────────────────────────────────────
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount     = true;
        options.Lockout.MaxFailedAccessAttempts   = 5;
        options.Lockout.DefaultLockoutTimeSpan    = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers        = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppIdentityDbContext>();

// ─────────────────────────────────────────────────────────────────
// M-05: COOKIE POLICY — SameSite + HttpOnly for CSRF protection
// Switch SecurePolicy to Always once you have a valid TLS cert.
// ─────────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite     = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // → Always in production
    options.Cookie.HttpOnly     = true;
});

builder.Services.AddTransient<IClaimsTransformation, DomainUserClaimsTransformation>();

// ─────────────────────────────────────────────────────────────────
// APPLICATION SERVICES — DI Registration
// ─────────────────────────────────────────────────────────────────

// ── Infrastructure ──────────────────────────────────────────────
builder.Services.AddScoped<IAuditService, AuditService>();

// ── Patient Management ──────────────────────────────────────────
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IPatientService, PatientService>();


// ── Clinical Recording ──────────────────────────────────────────
builder.Services.AddScoped<IVisitService, VisitService>();
// builder.Services.AddScoped<IVitalsService, VitalsService>(); // ← was missing
builder.Services.AddScoped<IBillingService, BillingService>();
// builder.Services.AddScoped<IPrescriptionService, PrescriptionService>(); // ← was missing
// builder.Services.AddScoped<IReferralService, ReferralService>(); // ← was missing
// builder.Services.AddScoped<IDiagnosticsService, DiagnosticsService>(); // ← was missing
builder.Services.AddScoped<ITestResultsService, TestResultService>(); 
// builder.Services.AddScoped<IMedAssistService, MedAssistService>();  // ← was missing

// ── Scheduling ──────────────────────────────────────────────────
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<ISchedulingQueryService, SchedulingQueryService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// ─────────────────────────────────────────────────────────────────
// M-02: SWAGGER — development only, never exposed in production
// ─────────────────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });
}

// ═════════════════════════════════════════════════════════════════
// HTTP PIPELINE
// Order matters. Each middleware only sees requests that reach it.
// ═════════════════════════════════════════════════════════════════
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
}

app.UseExceptionHandler(errorApp =>
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var statusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            FormatException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var expectsJson = context.Request.Path.StartsWithSegments("/api")
                          || context.Request.Headers.Accept.Any(header =>
                              header?.Contains("json", StringComparison.OrdinalIgnoreCase) == true);

        if (expectsJson)
        {
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                title = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Invalid request.",
                    StatusCodes.Status403Forbidden => "Forbidden.",
                    StatusCodes.Status404NotFound => "Not found.",
                    _ => "An unexpected error occurred."
                },
                status = statusCode,
                detail = exception?.Message
            });
            return;
        }

        if (statusCode is StatusCodes.Status403Forbidden or StatusCodes.Status404NotFound or StatusCodes.Status500InternalServerError)
        {
            context.Response.Redirect($"/Error/{statusCode}");
            return;
        }

        context.Response.Redirect("/Home/Error");
    }));

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// ─────────────────────────────────────────────────────────────────
// C-03: SECURITY HEADERS
//
// WHY THIS POSITION: Headers middleware MUST come before UseStaticFiles.
// UseStaticFiles short-circuits the pipeline for .css/.js/.png requests,
// so anything registered after it never runs for those responses.
// Placing headers here ensures every response — static or dynamic — gets them.
//
// WHY Headers["key"] = value INSTEAD OF Headers.Append("key", ...):
// Append adds another entry to the header list each time it's called.
// If any upstream middleware already wrote the same header (e.g., the error
// handler setting Content-Type, or a redirect), you end up with duplicates.
// Two Content-Security-Policy headers = browser applies the stricter of the two,
// which can silently block resources. The indexer sets exactly one value.
// ─────────────────────────────────────────────────────────────────
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"]  = "nosniff";
    context.Response.Headers["X-Frame-Options"]         = "DENY";
    context.Response.Headers["Referrer-Policy"]         = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"]      = "geolocation=(), microphone=(), camera=()";

    // unsafe-inline retained for Swagger UI / Razor tag helpers.
    // Once you move to a full SPA you can tighten this to a nonce-based policy.
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self';";

    await next(context);
});

// Static files come AFTER headers so they inherit the security headers above.
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter(); // C-02
app.UseCors("HmsPolicy"); // M-01 — must be after UseRouting, before UseAuthorization

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ═════════════════════════════════════════════════════════════════
// DATABASE SEED
// ═════════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger   = services.GetRequiredService<ILogger<Program>>(); // H-04

    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Seed roles
        foreach (var role in new[] { "Doctor", "Nurse", "Admin", "Secretary" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed test doctor
        var doctorEmail = "doctor@hospital.com";
        var doctorUser  = await userManager.FindByEmailAsync(doctorEmail);

        if (doctorUser == null)
        {
            doctorUser = new IdentityUser
            {
                UserName       = doctorEmail,
                Email          = doctorEmail,
                EmailConfirmed = true
            };

            // C-01: Password from configuration — never hardcode credentials
            var seedPassword = builder.Configuration["Seed:DoctorPassword"]
                ?? throw new InvalidOperationException(
                    "Seed:DoctorPassword not configured. Add it to appsettings.Development.json " +
                    "or as a Docker/environment secret.");

            var createResult = await userManager.CreateAsync(doctorUser, seedPassword);

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(doctorUser, "Doctor");
                logger.LogInformation("Seeded test doctor: {Email}", doctorEmail);
            }
            else
            {
                logger.LogError("Seed failed for {Email}: {Errors}", doctorEmail,
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        // Seed test patients
        var clinicContext = services.GetRequiredService<ClinicContext>();

        var linkedDoctor = await clinicContext.Doctors
            .FirstOrDefaultAsync(doctor => doctor.IdentityUserId == doctorUser.Id);

        if (linkedDoctor == null)
        {
            linkedDoctor = new Doctor
            {
                FirstName = "Seeded",
                LastName = "Doctor",
                Specialization = "General Practice",
                IdentityUserId = doctorUser.Id,
                IsTriageQualified = true
            };

            await clinicContext.Doctors.AddAsync(linkedDoctor);
            await clinicContext.SaveChangesAsync();
            logger.LogInformation("Seeded doctor profile linked to {Email}.", doctorEmail);
        }

        if (!clinicContext.Patients.Any())
        {
            var testPatients = new List<Patient>
            {
                new Patient
                {
                    PatientPublicId = SecureIdGenerator.GenerateID(15, "PA"),
                    FirstName       = "Travis",
                    LastName        = "Eweka",
                    DateOfBirth     = new DateOnly(2000, 5, 15),
                    Gender          = "Male",
                    PhoneNumber     = "555-0100",
                    HealthCardNo    = "1234567890-TR",
                    Type            = "Enrolled",
                    Email           = "travis@example.com",
                },
                new Patient
                {
                    PatientPublicId = SecureIdGenerator.GenerateID(15, "PA"),
                    FirstName       = "Sarah",
                    LastName        = "Connor",
                    DateOfBirth     = new DateOnly(1985, 2, 28),
                    Gender          = "Female",
                    PhoneNumber     = "555-0101",
                    HealthCardNo    = "0987654321-SA",
                    Type            = "Walk-in",
                    Email           = "sarah.c@example.com",
                },
                new Patient
                {
                    PatientPublicId = SecureIdGenerator.GenerateID(15, "PA"),
                    FirstName       = "Bruce",
                    LastName        = "Wayne",
                    DateOfBirth     = new DateOnly(1990, 11, 3),
                    Gender          = "Male",
                    PhoneNumber     = "555-0102",
                    Email           = "bruce.w@example.com",
                    HealthCardNo    = "1122334455-BR",
                    Type            = "Enrolled"
                }
            };

            await clinicContext.Patients.AddRangeAsync(testPatients);
            await clinicContext.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} test patients.", testPatients.Count);
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "A critical error occurred during database seed.");
    }
}

app.Run();
