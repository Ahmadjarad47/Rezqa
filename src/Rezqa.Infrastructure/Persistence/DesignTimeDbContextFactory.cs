using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Rezqa.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Get the project directory
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectDirectory = Path.Combine(currentDirectory, "..", "..", "src", "Rezqa.API");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(projectDirectory, "appsettings.json"), optional: false)
            .AddJsonFile(Path.Combine(projectDirectory, "appsettings.Development.json"), optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseSqlServer(connectionString);

        // Create a logger factory that does nothing for design-time
        var loggerFactory = LoggerFactory.Create(builder => { });

        return new ApplicationDbContext(builder.Options, loggerFactory.CreateLogger<ApplicationDbContext>());
    }
}