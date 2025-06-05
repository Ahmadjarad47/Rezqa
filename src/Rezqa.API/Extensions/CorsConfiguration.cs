using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Rezqa.API.Extensions;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("StrictPolicy", builder =>
            {
                // Load allowed origins from configuration or hardcode
                var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

                if (allowedOrigins == null || !allowedOrigins.Any())
                {
                    allowedOrigins = new[] { "http://localhost:4200" }; // ✅ Do NOT add trailing slash
                }

                builder.WithOrigins(allowedOrigins)
                       .AllowCredentials()
                       .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                       .WithHeaders("Authorization", "Content-Type", "X-XSRF-TOKEN")
                       .SetIsOriginAllowedToAllowWildcardSubdomains()
                       .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });


        return services;
    }

    public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder app)
    {
        app.UseCors("StrictPolicy");
        return app;
    }
}