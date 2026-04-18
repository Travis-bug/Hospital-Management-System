using System;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Hospital_Management_System.Data;
using Hospital_Management_System.Models;
using Hospital_Management_System.Services.ClinicalRecording;
using Hospital_Management_System.Services.Infrastructure;
using Hospital_Management_System.Services.PatientManagement;
using Hospital_Management_System.Services.Scheduling;
using Hospital_Management_System.Services.StaffManagement;
using Hospital_Management_System.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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

// SPA host + API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // EF entities expose bidirectional navigation properties, so API responses can
        // otherwise recurse infinitely when Swagger tries to serialize them.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });





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
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            title = "Too many requests.",
            status = StatusCodes.Status429TooManyRequests,
            detail = "Too many authentication attempts. Wait 5 minutes before retrying."
        }, cancellationToken);
    };
});

// ─────────────────────────────────────────────────────────────────
// M-01: CORS — explicit allowlist, never wildcard
// ─────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors:AllowedOrigins must be configured with explicit frontend origins.");
}

var frontendPublicBaseUrl = builder.Configuration["Frontend:PublicBaseUrl"]
    ?? builder.Configuration["App:PublicBaseUrl"];

if (!builder.Environment.IsDevelopment())
{
    foreach (var origin in allowedOrigins)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri) || originUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                $"Production CORS origin '{origin}' is not HTTPS. Only HTTPS origins are allowed outside development.");
        }
    }

    if (!string.IsNullOrWhiteSpace(frontendPublicBaseUrl)
        && (!Uri.TryCreate(frontendPublicBaseUrl, UriKind.Absolute, out var frontendUri)
            || frontendUri.Scheme != Uri.UriSchemeHttps))
    {
        throw new InvalidOperationException(
            $"Frontend:PublicBaseUrl '{frontendPublicBaseUrl}' is not HTTPS. Production activation links must use HTTPS.");
    }
}

builder.Services.AddCors(options =>
    options.AddPolicy("HmsPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
              .WithHeaders("Authorization", "Content-Type", "Accept")
              .AllowCredentials()));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

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
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

// ─────────────────────────────────────────────────────────────────
// M-05: COOKIE POLICY — SameSite + HttpOnly for CSRF protection
// ─────────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan       = TimeSpan.FromHours(1);
    options.SlidingExpiration    = true;
    options.Cookie.SameSite     = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly     = true;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddTransient<IClaimsTransformation, DomainUserClaimsTransformation>();

// ─────────────────────────────────────────────────────────────────
// APPLICATION SERVICES — DI Registration
// ─────────────────────────────────────────────────────────────────

// ── Infrastructure ──────────────────────────────────────────────
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

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

app.UseForwardedHeaders();

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

        // if (statusCode is StatusCodes.Status403Forbidden or StatusCodes.Status404NotFound or StatusCodes.Status500InternalServerError)
        //   {
           // context.Response.Redirect($"/Error/{statusCode}");
           // return;
       // }
       // context.Response.Redirect("/Home/Error");
       
        var pageTitle = statusCode switch
        {
            StatusCodes.Status400BadRequest => "The request could not be processed.",
            StatusCodes.Status403Forbidden => "You do not have access to this page.",
            StatusCodes.Status404NotFound => "The requested page was not found.",
            _ => "The application encountered an unexpected error."
        };

        var pageDetail = exception?.Message ?? "An unexpected server-side error occurred.";

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync($@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Application Error</title>
  <style>
    :root {{
      color-scheme: light;
      font-family: ""IBM Plex Sans"", ""Segoe UI"", sans-serif;
      background:
        radial-gradient(circle at top right, rgba(30, 58, 138, 0.14), transparent 22rem),
        radial-gradient(circle at left center, rgba(148, 163, 184, 0.2), transparent 20rem),
        linear-gradient(180deg, #f8fafc 0%, #eef2ff 100%);
    }}
    body {{
      margin: 0;
      min-height: 100vh;
      display: grid;
      place-items: center;
      padding: 24px;
      color: #0f172a;
    }}
    section {{
      width: min(100%, 720px);
      border: 1px solid rgba(148, 163, 184, 0.3);
      border-radius: 24px;
      background: rgba(255, 255, 255, 0.92);
      box-shadow: 0 24px 80px rgba(15, 23, 42, 0.12);
      padding: 32px;
    }}
    p.meta {{
      margin: 0;
      font-size: 12px;
      font-weight: 700;
      letter-spacing: 0.18em;
      text-transform: uppercase;
      color: #64748b;
    }}
    h1 {{
      margin: 12px 0 0;
      font-size: 32px;
      line-height: 1.15;
    }}
    p {{
      color: #475569;
      line-height: 1.7;
    }}
    a {{
      display: inline-block;
      margin-top: 8px;
      color: #1d4ed8;
      font-weight: 600;
      text-decoration: none;
    }}
  </style>
</head>
<body>
  <section>
    <p class=""meta"">Application Error</p>
    <h1>{pageTitle}</h1>
    <p>{pageDetail}</p>
    <a href=""/"">Return to the dashboard</a>
  </section>
</body>
</html>");
        return;
       
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
app.UseDefaultFiles();
app.UseStaticFiles();





app.UseRouting();

app.UseCors("HmsPolicy"); // M-01 — must be after UseRouting, before UseAuthorization
app.UseRateLimiter(); // C-02

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Maps your /api/ endpoints

// The SPA Fallback: Any route the API doesn't recognize gets sent to React!
app.MapFallbackToFile("index.html");





// ═════════════════════════════════════════════════════════════════
// DEMO IDENTITY SYNC
// SQL owns the clinic-side demo data. Startup only ensures those staff
// records have usable ASP.NET Identity accounts for local testing.
// ═════════════════════════════════════════════════════════════════
// Demo identity patching retired.
// Staff onboarding now creates and links Identity users directly.
// If the old local recovery patcher is ever needed again, the source has been
// moved out of the runtime project into the repository Garb folder.

app.Run();
