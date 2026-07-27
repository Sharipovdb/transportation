using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class TransportDayDatabaseSeeder : IDatabaseSeeder
{
    private readonly TransportationDbContext _context;

    public TransportDayDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }


    public async Task SeedAsync()
    {
        if (await _context.TransportDays.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var crews = await _context.Crews
            .OrderBy(x => x.Id)
            .Take(3)
            .ToListAsync();

        if (!crews.Any())
            throw new InvalidOperationException("No crews found. Run CrewDatabaseSeeder first.");

        var users = await _context.Users.ToDictionaryAsync(x => x.UserName!, x => x.Id);

        long GetUser(string username)
        {
            if (!users.TryGetValue(username, out var id))
                throw new InvalidOperationException($"User '{username}' not found.");

            return id;
        }

        var transportDays = new List<TransportDay>
        {
            new()
            {
                CrewId = crews[0].Id,
                Date = DateTime.UtcNow.Date,
                MorningMode = TransportMode.Driven,
                AfternoonMode = TransportMode.Driven,
                BaseRouteKm = 18,
                ExtraCommuteKm = 0,
                ExtraBusinessKm = 0,
                DriverId = GetUser("rustam.driver"),
                Notes = "Normal working day",
                LoggedBy = GetUser("bekzod.manager"),
                LoggedAt = now,
                Confirmed = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crews[0].Id,
                Date = DateTime.UtcNow.Date.AddDays(1),
                MorningMode = TransportMode.Taxi,
                AfternoonMode = TransportMode.Driven,
                BaseRouteKm = 18,
                ExtraCommuteKm = 2,
                ExtraBusinessKm = 5,
                DriverId = GetUser("rustam.driver"),
                Notes = "Morning taxi because vehicle problem",
                LoggedBy = GetUser("bekzod.manager"),
                LoggedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crews[1].Id,
                Date = DateTime.UtcNow.Date,
                MorningMode = TransportMode.Driven,
                AfternoonMode = TransportMode.Taxi,
                BaseRouteKm = 25,
                ExtraCommuteKm = 0,
                ExtraBusinessKm = 10,
                DriverId = GetUser("diyor.driver"),
                Notes = "Afternoon taxi",
                LoggedBy = GetUser("sardor.lead"),
                LoggedAt = now,
                Confirmed = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crews[2].Id,
                Date = DateTime.UtcNow.Date.AddDays(2),
                MorningMode = TransportMode.None,
                AfternoonMode = TransportMode.Driven,
                BaseRouteKm = 15,
                ExtraCommuteKm = 3,
                ExtraBusinessKm = 7,
                DriverId = GetUser("rustam.driver"),
                Notes = "Late start due to weather",
                LoggedBy = GetUser("javlon.lead"),
                LoggedAt = now,
                Confirmed = false,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };

        await _context.TransportDays.AddRangeAsync(transportDays);

        await _context.SaveChangesAsync();
    }
}