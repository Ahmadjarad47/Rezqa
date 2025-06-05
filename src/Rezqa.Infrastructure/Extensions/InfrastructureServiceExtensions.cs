using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Rezqa.Domain.Common.Interfaces;
using Rezqa.Infrastructure.Persistence;
using Rezqa.Infrastructure.Services;
using Rezqa.Infrastructure.Settings;

namespace Rezqa.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register FileService with IWebHostEnvironment
        services.AddScoped<IFileService, FileService>();

        // Register Email Service
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        services.AddSingleton(emailSettings ?? throw new InvalidOperationException("Email settings are not configured."));
        services.AddScoped<IEmailService, EmailService>();

        // Register EF DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        return services;
    }
}