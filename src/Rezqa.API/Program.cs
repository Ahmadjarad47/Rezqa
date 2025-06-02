using Rezqa.Application;
using Rezqa.API.Extensions;
using Rezqa.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Rezqa.Infrastructure.Persistence;
using Rezqa.Application.Features.User.Handlers.Commands;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Rezqa.Application.Features.User.Settings;
using Rezqa.Infrastructure.Settings;
using Rezqa.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Add antiforgery token validation for all POST, PUT, DELETE requests
    //options.Filters.Add<ValidateAntiforgeryTokenAttribute>();

    // Add response caching
    options.CacheProfiles.Add("Default30",
        new CacheProfile()
        {
            Duration = 30,
            Location = ResponseCacheLocation.Any,
            VaryByQueryKeys = new[] { "*" }
        });
});

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Memory Cache
builder.Services.AddMemoryCache();

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

// Add Identity with optimized settings
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
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
    options.Tokens.ProviderMap["Default"] = new TokenProviderDescriptor(typeof(DataProtectorTokenProvider<IdentityUser>));
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT with optimized settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

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
        ValidIssuer = jwtSettings!.Issuer,
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

builder.Services.AddRateLimiting(builder.Configuration);

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Response Compression
app.UseResponseCompression();

// Use Response Caching
app.UseResponseCaching();

// Use custom security middleware
app.UseSecurityMiddleware();
app.UseCorsMiddleware();

// Use XSS Protection Middleware
app.UseMiddleware<XssProtectionMiddleware>();

// Enforce HTTPS redirection
app.UseHttpsRedirection();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiting();

// Map controller endpoints
app.MapControllers();

app.Run();
