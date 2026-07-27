using Microsoft.AspNetCore.Identity;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

// Demo password shared by every seeded account (satisfies the default Identity
// password policy: length >= 6, upper/lower/digit/non-alphanumeric).
internal static class DemoCredentials
{
    public const string Password = "Passw0rd!";
}

internal sealed class UserDatabaseSeeder : IDatabaseSeeder
{
    private readonly UserManager<User> _userManager;

    public UserDatabaseSeeder(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        var users = new List<User>
        {
            new()
            {
                UserName = "admin",
                FirstName = "System",
                LastName = "Admin",
                PhoneNumber = "+992900000000",
                TelegramId = "10000",
                Email = "admin@crewmove.com",
                EmailConfirmed = true
            },

            new()
            {
                UserName = "bekzod.manager",
                FirstName = "Bekzod",
                LastName = "Karimov",
                TelegramId = "900000001",
                PhoneNumber = "+992900000001",
            },

            new()
            {
                UserName = "sardor.lead",
                PhoneNumber = "+992900000002",
                FirstName = "Sardor",
                LastName = "Rahmonov",
                TelegramId = "10002",
                Email = "sardor@crewmove.com",
                EmailConfirmed = true
            },

            new()
            {
                UserName = "javlon.lead",
                PhoneNumber = "+992900000003",
                FirstName = "Javlon",
                LastName = "Nazarov",
                TelegramId = "10003",
                Email = "javlon@crewmove.com",
                EmailConfirmed = true
            },

            new()
            {
                UserName = "rustam.driver",
                PhoneNumber = "+992900000004",
                FirstName = "Rustam",
                LastName = "Aliyev",
                TelegramId = "10004",
                Email = "rustam@crewmove.com",
                EmailConfirmed = true
            },

            new()
            {
                UserName = "diyor.driver",
                PhoneNumber = "+992900000005",
                FirstName = "Diyor",
                LastName = "Safarov",
                TelegramId = "10005",
                Email = "diyor@crewmove.com",
                EmailConfirmed = true
            },

            new()
            {
                UserName = "worker.01",
                PhoneNumber = "+992900000006",
                FirstName = "Aziz",
                LastName = "Yusupov",
                TelegramId = "10006"
            },

            new()
            {
                UserName = "worker.02",
                PhoneNumber = "+992900000007",
                FirstName = "Sherzod",
                LastName = "Tursunov",
                TelegramId = "10007"
            },

            new()
            {
                UserName = "worker.03",
                PhoneNumber = "+992900000008",
                FirstName = "Kamron",
                LastName = "Abdulloev",
                TelegramId = "10008"
            },

            new()
            {
                UserName = "worker.04",
                PhoneNumber = "+992900000009",
                FirstName = "Islom",
                LastName = "Homidov",
                TelegramId = "10009"
            },

            new()
            {
                UserName = "worker.05",
                PhoneNumber = "+992900000010",
                FirstName = "Farruh",
                LastName = "Qodirov",
                TelegramId = "10010"
            },

            new()
            {
                UserName = "worker.06",
                PhoneNumber = "+992900000011",
                FirstName = "Muhammad",
                LastName = "Karimzoda",
                TelegramId = "10011"
            },

            new()
            {
                UserName = "worker.07",
                PhoneNumber = "+992900000012",
                FirstName = "Dilshod",
                LastName = "Nasriddinov",
                TelegramId = "10012"
            },

            new()
            {
                UserName = "worker.08",
                PhoneNumber = "+992900000013",
                FirstName = "Shoxrux",
                LastName = "Valiyev",
                TelegramId = "10013"
            },

            new()
            {
                UserName = "worker.09",
                PhoneNumber = "+992900000014",
                FirstName = "Fayzullo",
                LastName = "Rasulov",
                TelegramId = "10014"
            },

            new()
            {
                UserName = "worker.10",
                PhoneNumber = "+992900000015",
                FirstName = "Abdullo",
                LastName = "Ismoilov",
                TelegramId = "10015"
            },

            new()
            {
                UserName = "accountant",
                PhoneNumber = "+992900000016",
                FirstName = "Malika",
                LastName = "Sharipova",
                TelegramId = "10016",
                Email = "accountant@crewmove.com",
                EmailConfirmed = true
            }
        };

        foreach (var user in users)
        {
            await EnsureUserExistsAsync(user);
        }
    }

    // Per-user idempotent: creates the account if missing, and backfills a password
    // for accounts that exist but were seeded (by an older version of this seeder)
    // without one — safe to run on every startup.
    private async Task EnsureUserExistsAsync(User user)
    {
        var existing = await _userManager.FindByNameAsync(user.UserName!);

        if (existing is null)
        {
            await _userManager.CreateAsync(user, DemoCredentials.Password);
            return;
        }

        if (!await _userManager.HasPasswordAsync(existing))
        {
            await _userManager.AddPasswordAsync(existing, DemoCredentials.Password);
        }
    }
}
