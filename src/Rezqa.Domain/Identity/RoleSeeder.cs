using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Rezqa.Domain.Identity;

public static class RoleSeeder
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Moderator = "Moderator";
    }

    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        try
        {
            // Check if roles exist, if not create them
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
                logger.LogInformation("Created {Role} role", Roles.Admin);
            }

            if (!await roleManager.RoleExistsAsync(Roles.User))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.User));
                logger.LogInformation("Created {Role} role", Roles.User);
            }

            if (!await roleManager.RoleExistsAsync(Roles.Moderator))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Moderator));
                logger.LogInformation("Created {Role} role", Roles.Moderator);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding roles");
            throw;
        }
    }

    public static async Task SeedAdminUserAsync(
        UserManager<IdentityUser> userManager,
        ILogger logger)
    {
        const string adminEmail = "admin@rezqa.com";
        const string adminPassword = "Admin123!@#";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                logger.LogInformation("Created admin user with email {Email}", adminEmail);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create admin user: {Errors}", errors);
                throw new ApplicationException($"Failed to create admin user: {errors}");
            }
        }
    }
} 