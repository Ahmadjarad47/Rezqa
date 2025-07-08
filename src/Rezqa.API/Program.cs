using Rezqa.Application;
using Rezqa.API.Extensions;
using Rezqa.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Rezqa.Infrastructure.Persistence;
using Rezqa.Infrastructure.Extensions;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Rezqa.Application.Features.User.Settings;
using Rezqa.API.RateLimiting;
using Rezqa.Domain.Entities;
using Rezqa.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); // تم التعطيل لأن Web API لا يحتاجها
});
builder.Services.AddSingleton<PresenceTracker>();
// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Layer
builder.Services.AddApplication();

// Add Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Add Advanced Rate Limiting
builder.Services.AddAdvancedRateLimiting(builder.Configuration);

// Add Identity with optimized settings
builder.Services.AddIdentity<AppUsers, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;

    // Optimize token providers
    options.Tokens.ProviderMap["Default"] = new TokenProviderDescriptor(typeof(DataProtectorTokenProvider<AppUsers>));
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddSingleton<PresenceMessageTracker>();
builder.Services.AddHostedService<Rezqa.API.Services.ConnectionCleanupService>();
builder.Services.AddHostedService<Rezqa.API.Services.AdExpirationService>();
// Configure JWT with environment variables
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "^&*^&ASdhqwebqwhjbej*&^&^$%^#$@#@#@$^&(_)*(_*)*&&*)%(%^^%&%^$nadsjknadsiuiu";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "http://localhost:7109";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "https://syrianopenstor.netlify.app/";
var jwtExpirationMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "60");

// تحقق من قوة JWT Secret
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Contains("your-super-secret"))
{
    throw new Exception("JWT_SECRET environment variable must be set to a strong value in production.");
}

var jwtSettings = new JwtSettings
{
    Secret = jwtSecret,
    Issuer = jwtIssuer,
    Audience = jwtAudience,
    ExpirationInMinutes = jwtExpirationMinutes
};

builder.Services.Configure<JwtSettings>(options =>
{
    options.Secret = jwtSettings.Secret;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpirationInMinutes = jwtSettings.ExpirationInMinutes;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret)
        ),
        ClockSkew = TimeSpan.Zero
    };

    // Configure JWT events to read token from cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // First try to get the token from the Authorization header
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // If not found in header, try to get from access token cookie
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["accessToken"];
            }

            context.Token = token;
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Add Security and CORS
builder.Services.AddSecurityServices(builder.Configuration);
builder.Services.AddCorsServices(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

var app = builder.Build();

// Ensure uploads directory exists
var fileBaseDirectory = Environment.GetEnvironmentVariable("FILE_BASE_DIRECTORY") ?? "uploads";
var webRootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var uploadsPath = Path.Combine(webRootPath, fileBaseDirectory);
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<AppUsers>>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        await context.SeedDataAsync(roleManager, userManager, logger);

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
// Use Response Compression
app.UseResponseCompression();

// Use custom security middleware
app.UseSecurityMiddleware();
app.UseCorsMiddleware();

app.UseSecurityMiddleware();
app.UseMiddleware<XssProtectionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Use Advanced Rate Limiting
app.UseAdvancedRateLimiting();

// Map controller endpoints
app.MapControllers();
app.MapHub<Messages>("/hubs/messages");
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
