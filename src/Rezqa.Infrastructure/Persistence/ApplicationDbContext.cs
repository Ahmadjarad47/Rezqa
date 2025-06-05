using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rezqa.Domain.Entities;
using Rezqa.Domain.Identity;

namespace Rezqa.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly ILogger<ApplicationDbContext>? _logger;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext>? logger = null)
        : base(options)
    {
        _logger = logger;
        // Configure for better performance
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Enable split queries for better performance
        //optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Image).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}

// Move seeding logic to a separate service
public static class DbContextExtensions
{
    public static async Task SeedDataAsync(
        this ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        ILogger logger)
    {
        try
        {
            await RoleSeeder.SeedRolesAsync(roleManager, logger);
            await RoleSeeder.SeedAdminUserAsync(userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}