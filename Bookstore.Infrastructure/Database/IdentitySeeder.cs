using Bookstore.Application.Constants;
using Bookstore.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Bookstore.Infrastructure.Database;

internal sealed class IdentitySeeder(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<IdentitySeeder> logger) : IIdentitySeeder
{
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedUserAsync("reader", "Reader123!", Roles.Read);
        await SeedUserAsync("admin", "Admin123!", Roles.ReadWrite);
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = [Roles.Read, Roles.ReadWrite];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Seeded role {Role}", role);
            }
        }
    }

    private async Task SeedUserAsync(string username, string password, string role)
    {
        if (await userManager.FindByNameAsync(username) is not null)
        {
            return;
        }

        var user = new IdentityUser { UserName = username };
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation("Seeded user {Username} with role {Role}", username, role);
        }
        else
        {
            logger.LogError("Failed to seed user {Username}: {Errors}",
                username, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
