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
                options.Cookie.Name = "accessToken";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;


                options.SlidingExpiration = true;
            });

        return services;
    }

    public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app)
    {

        // Add Security Headers
        app.Use(async (context, next) =>
        {
            // Content Security Policy
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; font-src 'self'; frame-ancestors 'none'; form-action 'self'; base-uri 'self'; object-src 'none'";

            // Strict Transport Security
            context.Response.Headers["Strict-Transport-Security"] =
                "max-age=31536000; includeSubDomains; preload";

            // X-Frame-Options
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // X-Content-Type-Options
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // Referrer Policy
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // X-XSS-Protection
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            await next();
        });

        return app;
    }
}