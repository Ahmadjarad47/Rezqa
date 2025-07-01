using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Rezqa.Domain.Common.Interfaces;
using Rezqa.Infrastructure.Persistence;
using Rezqa.Infrastructure.Persistence.Repositories;
using Rezqa.Infrastructure.Services;
using Rezqa.Infrastructure.Settings;
using Rezqa.Application.Interfaces;

namespace Rezqa.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register CloudinaryService with IFileService interface
        services.AddScoped<IFileService, CloudinaryService>();

        // Register Email Service with environment variables
        var emailSettings = new EmailSettings
        {
            SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "smtp.gmail.com",
            SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587"),
            SmtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "ahmad222jarad@gmail.com",
            SmtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "ejaswcruyvmuksyy",
            FromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? "ahmad222jarad@gmail.com",
            FromName = Environment.GetEnvironmentVariable("FROM_NAME") ?? "Rezqa",
            BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "https://syrianopenstor.netlify.app/"
        };

        services.AddSingleton(emailSettings);
        services.AddScoped<IEmailService, EmailService>();

        // Register Repositories
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
        services.AddScoped<IDynamicFieldRepository, DynamicFieldRepository>();
        services.AddScoped<IAdRepository, AdRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();

        // Register EF DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        return services;
    }
}