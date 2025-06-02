using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;

namespace Rezqa.API.Extensions;

public static class SecurityConfiguration
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Rate Limiting
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        services.AddInMemoryRateLimiting();

        // Add Antiforgery
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "XSRF-TOKEN";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.HeaderName = "X-XSRF-TOKEN";
        });

        // Add Cookie Authentication with secure settings
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

        return services;
    }

    public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app)
    {
        // Use Rate Limiting
        app.UseIpRateLimiting();

        // Add Security Headers
        app.Use(async (context, next) =>
        {
            // Content Security Policy
            context.Response.Headers.Add(
                "Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self'; " +
                "frame-ancestors 'none'; " +
                "form-action 'self'; " +
                "base-uri 'self'; " +
                "object-src 'none'");

            // Strict Transport Security
            context.Response.Headers.Add(
                "Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload");

            // X-Frame-Options
            context.Response.Headers.Add(
                "X-Frame-Options",
                "DENY");

            // X-Content-Type-Options
            context.Response.Headers.Add(
                "X-Content-Type-Options",
                "nosniff");

            // Referrer Policy
            context.Response.Headers.Add(
                "Referrer-Policy",
                "strict-origin-when-cross-origin");

            // X-XSS-Protection
            context.Response.Headers.Add(
                "X-XSS-Protection",
                "1; mode=block");

            await next();
        });

        return app;
    }
} 