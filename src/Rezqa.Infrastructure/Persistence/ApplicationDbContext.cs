using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rezqa.Domain.Common;
using Rezqa.Domain.Entities;
using Rezqa.Domain.Identity;

namespace Rezqa.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<AppUsers, IdentityRole<Guid>, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>
         , IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{

    private readonly IHttpContextAccessor _httpContextAccessor;
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
       IHttpContextAccessor httpContextAccessor)
        : base(options)
    {

        // Configure for better performance
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<SubCategory> SubCategories { get; set; } = null!;
    public DbSet<DynamicField> DynamicFields { get; set; } = null!;
    public DbSet<FieldOption> FieldOptions { get; set; } = null!;

    public DbSet<Ad> ads { get; set; } = null!;
    public DbSet<AdFieldValue> adFieldValues { get; set; } = null!;

    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<IdentityUserLogin<Guid>>()
            .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        builder.Entity<IdentityUserRole<Guid>>()
           .HasKey(l => new { l.UserId, l.RoleId });

        builder.Entity<IdentityUserToken<Guid>>()
           .HasKey(l => new { l.UserId, l.LoginProvider, l.Name });

        // Configure SubCategory entity
        builder.Entity<SubCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(builder);
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Enable split queries for better performance
        //optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "not";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow; // Use UTC for consistency
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUser!;
                    entry.Entity.UpdatedBy = currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUser;
                    break;

                default:
                    break;
            }
        }

        // Handle SubCategory audit fields
        foreach (var entry in ChangeTracker.Entries<SubCategory>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUser!;
                    entry.Entity.UpdatedBy = currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUser;
                    break;

                default:
                    break;
            }
        }

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Log or handle exception as needed
            var errorMessage = $"Error in SaveChangesAsync: {ex.Message}. " +
                               $"InnerException: {ex.InnerException?.Message}";
            Console.WriteLine(errorMessage); // Replace with your logging library
            throw new Exception("An error occurred while saving changes to the database.", ex);
        }
    }

}

// Move seeding logic to a separate service
public static class DbContextExtensions
{
    public static async Task SeedDataAsync(
        this ApplicationDbContext context,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<AppUsers> userManager,
        ILogger logger)
    {
        try
        {
            await RoleSeeder.SeedRolesAsync(roleManager, logger);
            await RoleSeeder.SeedAdminUserAsync(userManager, roleManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}