using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rezqa.Domain.Entities;

namespace Rezqa.Domain.Identity;

public static class RoleSeeder
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Moderator = "Moderator";
    }

    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
    {
        logger.LogInformation("Starting role seeding process...");

        var rolesToSeed = new[] { Roles.Admin, Roles.User, Roles.Moderator };

        foreach (var roleName in rolesToSeed)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var role = new IdentityRole<Guid>(roleName);
                var result = await roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    logger.LogInformation("Successfully created {Role} role", roleName);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create {Role} role: {Errors}", roleName, errors);
                    throw new ApplicationException($"Failed to create {roleName} role: {errors}");
                }
            }
            else
            {
                logger.LogInformation("Role {Role} already exists", roleName);
            }
        }

        logger.LogInformation("Role seeding completed successfully");
    }

    public static async Task SeedAdminUserAsync(
        UserManager<AppUsers> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger logger)
    {
        const string adminEmail = "admin@rezqa.com";
        const string adminPassword = "Admin123!@#";
        const string adminUsername = "admin";

        logger.LogInformation("Starting admin user seeding process...");

        // Ensure admin role exists
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await SeedRolesAsync(roleManager, logger);
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUsers
            {
                Id = Guid.NewGuid(),
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                LockoutEnabled = true,
                image = "-"
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, Roles.Admin);

                if (addToRoleResult.Succeeded)
                {
                    logger.LogInformation("Successfully created admin user {Username} and added to Admin role", adminUsername);
                }
                else
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to add admin user to role: {Errors}", errors);
                    throw new ApplicationException($"Failed to add admin user to role: {errors}");
                }
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create admin user: {Errors}", errors);
                throw new ApplicationException($"Failed to create admin user: {errors}");
            }
        }
        else
        {
            // Ensure existing admin user has the Admin role
            if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                logger.LogInformation("Added Admin role to existing admin user");
            }
            logger.LogInformation("Admin user already exists");
        }

        logger.LogInformation("Admin user seeding completed successfully");
    }
}