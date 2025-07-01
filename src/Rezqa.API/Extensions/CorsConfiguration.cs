using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Rezqa.API.Extensions;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Get allowed origins from environment variable
        var allowedOrigins = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") ?? "https://syrianopenstor.netlify.app").Split(';');

        services.AddCors(config =>
        {
            config.AddPolicy("StrictPolicy", op =>
            {
                op.SetIsOriginAllowed(or => allowedOrigins.Contains(or))
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
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