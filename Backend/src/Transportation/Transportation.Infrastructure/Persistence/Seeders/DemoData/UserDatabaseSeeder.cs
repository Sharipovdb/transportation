using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class UserDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public UserDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 2;

    public async Task SeedAsync()
    {
        var users = GetUsers();

        foreach (var user in users)
        {
            var exists = await _context.Users.AnyAsync(x => x.UserName == user.UserName);

            if (exists)
                continue;

            user.NormalizedUserName = user.UserName!.ToUpperInvariant();

            user.NormalizedEmail = user.Email?.ToUpperInvariant();

            user.SecurityStamp = Guid.NewGuid().ToString();

            user.ConcurrencyStamp = Guid.NewGuid().ToString();

            await _context.Users.AddAsync(user);
        }

        await _context.SaveChangesAsync();
    }

    private static List<User> GetUsers()
    {
        return
        [
            new()
            {
                UserName = "bekzod.manager",
                FirstName = "Bekzod",
                LastName = "Karimov",
                TelegramId = "900000001",
                Email = "beckzod@crewmove.com",
                PhoneNumber = "+992900000001"
            },
            new()
            {
                UserName = "sardor.lead",
                FirstName = "Sardor",
                LastName = "Rahmonov",
                TelegramId = "10002",
                Email = "sardor@crewmove.com",
                PhoneNumber = "+992900000002",
                EmailConfirmed = true
            },
            new()
            {
                UserName = "javlon.lead",
                FirstName = "Javlon",
                LastName = "Nazarov",
                TelegramId = "10003",
                Email = "javlon@crewmove.com",
                PhoneNumber = "+992900000003",
                EmailConfirmed = true
            },
            new()
            {
                UserName = "rustam.driver",
                FirstName = "Rustam",
                LastName = "Aliyev",
                TelegramId = "10004",
                Email = "rustam@crewmove.com",
                PhoneNumber = "+992900000004",
                EmailConfirmed = true
            },
            new()
            {
                UserName = "diyor.driver",
                FirstName = "Diyor",
                LastName = "Safarov",
                TelegramId = "10005",
                Email = "diyor@crewmove.com",
                PhoneNumber = "+992900000005",
                EmailConfirmed = true
            },
            new()
            {
                UserName = "worker.01",
                FirstName = "Aziz",
                LastName = "Yusupov",
                TelegramId = "10006",
                Email = "aziz@crewmove.com",
                PhoneNumber = "+992900000006"
            },
            new()
            {
                UserName = "worker.02",
                FirstName = "Sherzod",
                LastName = "Tursunov",
                TelegramId = "10007",
                Email = "sherzod@crewmove.com",
                PhoneNumber = "+992900000007"
            },
            new()
            {
                UserName = "worker.03",
                FirstName = "Kamron",
                LastName = "Abdulloev",
                TelegramId = "10008",
                Email = "kamron@crewmove.com",
                PhoneNumber = "+992900000008"
            },
            new()
            {
                UserName = "worker.04",
                FirstName = "Islom",
                LastName = "Homidov",
                TelegramId = "10009",
                Email = "islom@crewmove.com",
                PhoneNumber = "+992900000009"
            },
            new()
            {
                UserName = "worker.05",
                FirstName = "Farruh",
                LastName = "Qodirov",
                TelegramId = "10010",
                Email = "farruh@crewmove.com",
                PhoneNumber = "+992900000010"
            },
            new()
            {
                UserName = "worker.06",
                FirstName = "Muhammad",
                LastName = "Karimzoda",
                TelegramId = "10011",
                Email = "muhammad@crewmove.com",
                PhoneNumber = "+992900000011"
            },
            new()
            {
                UserName = "worker.07",
                FirstName = "Dilshod",
                LastName = "Nasriddinov",
                TelegramId = "10012",
                Email = "dilshod@crewmove.com",
                PhoneNumber = "+992900000012"
            },
            new()
            {
                UserName = "worker.08",
                FirstName = "Shoxrux",
                LastName = "Valiyev",
                TelegramId = "10013",
                Email = "shoxrux@crewmove.com",
                PhoneNumber = "+992900000013"
            },
            new()
            {
                UserName = "worker.09",
                FirstName = "Fayzullo",
                LastName = "Rasulov",
                TelegramId = "10014",
                Email = "fayzullo@crewmove.com",
                PhoneNumber = "+992900000014"
            },
            new()
            {
                UserName = "worker.10",
                FirstName = "Abdullo",
                LastName = "Ismoilov",
                TelegramId = "10015",
                Email = "abdullo@crewmove.com",
                PhoneNumber = "+992900000015"
            }
        ];
    }
}