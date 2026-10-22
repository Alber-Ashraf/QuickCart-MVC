using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickCart.Data;
using QuickCart.DataAccess.DbInitializer;
using QuickCart.DataAccess.Repository;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Utility;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Global Anti-CSRF protection across all state-changing POST/PUT/DELETE requests
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddDbContext<QuickCartDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Health checks registration for database liveness and readiness
builder.Services.AddHealthChecks()
    .AddDbContextCheck<QuickCartDbContext>("sql_server_database", tags: new[] { "ready", "db" });

// Hardened ASP.NET Core Identity configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Lockout settings for brute-force defense
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    // User requirements
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<QuickCartDbContext>()
.AddDefaultTokenProviders();

// Hardened application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Multi-provider OAuth Configuration (Google, Facebook) adapted from Flexfolio
builder.Services.AddHttpClient();
builder.Services.Configure<OAuthSettings>(builder.Configuration.GetSection("OAuthSettings"));
builder.Services.AddScoped<IOAuthService, OAuthService>();

var authBuilder = builder.Services.AddAuthentication();

// 1. Google OAuth
var googleClientId = builder.Configuration["OAuthSettings:GoogleClientId"]
    ?? builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["OAuthSettings:GoogleClientSecret"]
    ?? builder.Configuration["Authentication:Google:ClientSecret"];
var googleCallbackPath = builder.Configuration["OAuthSettings:GoogleCallbackPath"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret) && !googleClientId.Contains("YOUR_GOOGLE"))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        if (!string.IsNullOrEmpty(googleCallbackPath))
        {
            options.CallbackPath = new PathString(googleCallbackPath);
        }
        options.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                var uri = context.RedirectUri;
                // Ensure public non-localhost domains always use https:// in the redirect_uri query parameter
                if (!context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                    !context.Request.Host.Host.Equals("127.0.0.1", StringComparison.Ordinal))
                {
                    uri = uri.Replace("redirect_uri=http%3A%2F%2F", "redirect_uri=https%3A%2F%2F", StringComparison.OrdinalIgnoreCase);
                }
                context.Response.Redirect(uri);
                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(context.Failure, "Google OAuth remote failure: {Message}", context.Failure?.Message);
                var errorMsg = Uri.EscapeDataString("Google login was canceled or failed: " + (context.Failure?.Message ?? "Authentication error"));
                context.Response.Redirect($"/Identity/Account/Login?ExternalLoginError={errorMsg}");
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}

// 2. Facebook OAuth
var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret) && !facebookAppId.Contains("YOUR_FACEBOOK"))
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = facebookAppId;
        options.AppSecret = facebookAppSecret;
        options.Events = new OAuthEvents
        {
            OnRemoteFailure = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(context.Failure, "Facebook OAuth remote failure: {Message}", context.Failure?.Message);
                var errorMsg = Uri.EscapeDataString("Facebook login was canceled or failed: " + (context.Failure?.Message ?? "Authentication error"));
                context.Response.Redirect($"/Identity/Account/Login?ExternalLoginError={errorMsg}");
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}

// Reverse proxy & forwarded headers support (RunASP.NET, MonsterASP, Azure, Linux Nginx, Docker)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// DataProtection key persistence to disk across AppPool recycles and restarts
var dataProtectionKeysFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
if (!Directory.Exists(dataProtectionKeysFolder))
{
    Directory.CreateDirectory(dataProtectionKeysFolder);
}
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysFolder))
    .SetApplicationName("Flyleaf");

// Session configuration with security flags
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Backblaze B2 S3-Compatible Storage configuration and registration
builder.Services.Configure<BackblazeSettings>(builder.Configuration.GetSection("Backblaze"));
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<BackblazeSettings>>().Value;
    var accessKey = string.IsNullOrWhiteSpace(settings.AccessKey) ? "placeholder-key" : settings.AccessKey;
    var secretKey = string.IsNullOrWhiteSpace(settings.SecretKey) ? "placeholder-secret" : settings.SecretKey;
    var credentials = new BasicAWSCredentials(accessKey, secretKey);

    var config = new AmazonS3Config
    {
        ServiceURL = string.IsNullOrWhiteSpace(settings.Endpoint)
            ? $"https://s3.{(string.IsNullOrWhiteSpace(settings.Region) ? "us-east-005" : settings.Region)}.backblazeb2.com"
            : settings.Endpoint,
        ForcePathStyle = true
    };

    return new AmazonS3Client(credentials, config);
});
builder.Services.AddScoped<BackblazeStorageService>();
builder.Services.AddScoped<IBlobService>(sp => sp.GetRequiredService<BackblazeStorageService>());
builder.Services.AddScoped<IStorageService>(sp => sp.GetRequiredService<BackblazeStorageService>());

var app = builder.Build();

// Forwarded headers MUST run first before scheme inspection, HTTPS redirection, and Auth
app.UseForwardedHeaders();

// Correlation ID & Structured Logging Scope Middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
    context.Response.Headers.Append("X-Correlation-ID", correlationId);

    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    using (logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId,
        ["RequestMethod"] = context.Request.Method,
        ["RequestPath"] = context.Request.Path.Value ?? ""
    }))
    {
        await next();
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Customer/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// OWASP Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://code.jquery.com https://cdn.datatables.net https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net https://cdn.datatables.net https://cdnjs.cloudflare.com; " +
        "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
        "img-src 'self' data: https: blob:; " +
        "connect-src 'self' https://api.stripe.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://cdn.datatables.net; " +
        "frame-src 'self' https://js.stripe.com https://hooks.stripe.com https://accounts.google.com;");

    await next();
});

app.UseStaticFiles();

// Safe Stripe initialization
var stripeSecret = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrEmpty(stripeSecret) && !stripeSecret.Contains("YOUR_STRIPE"))
{
    StripeConfiguration.ApiKey = stripeSecret;
}

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Operational Health Check Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds + "ms",
            timestamp = DateTime.UtcNow.ToString("o"),
            entries = report.Entries.Select(e => new
            {
                check = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds + "ms",
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
});

SeedDatabase();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedDatabase()
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    try
    {
        dbInitializer.Initialize();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
