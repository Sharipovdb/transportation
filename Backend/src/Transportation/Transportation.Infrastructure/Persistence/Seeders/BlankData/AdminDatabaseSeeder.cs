using Microsoft.AspNetCore.Identity;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;
using Transportation.Shared.Authorization;

namespace Transportation.Infrastructure.Persistence.Seeders.BlankData;

internal sealed class AdminDatabaseSeeder : IBlankDataSeeder
{
    private readonly UserManager<User> _userManager;

    public AdminDatabaseSeeder(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public int Order => 2;

    public async Task SeedAsync()
    {
        const string username = "admin";
        const string email = "admin@crewmove.local";
        const string password = "Admin123!";

        var existingAdmin = await _userManager.FindByNameAsync(username);

        if (existingAdmin is not null)
            return;

        var admin = new User
        {
            UserName = username,
            Email = email,
            FirstName = "System",
            LastName = "Administrator",
            PhoneNumber = "+998900000000",
            TelegramId = "admin"
        };

        var createResult = await _userManager.CreateAsync(admin, password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));

            throw new InvalidOperationException($"Failed to create admin user. Errors: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(admin, RoleNames.Admin);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(x => x.Description));

            throw new InvalidOperationException($"Failed to assign Admin role. Errors: {errors}");
        }

        Console.WriteLine(username + "  " + password);
    }
}