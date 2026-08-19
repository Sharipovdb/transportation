using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class TransportDayDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public TransportDayDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 8;

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        var crews = await _context.Crews
            .ToDictionaryAsync(x => x.Name, x => x.Id);

        var users = await _context.Users
            .ToDictionaryAsync(x => x.UserName!, x => x.Id);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await CreateTransportDayIfNotExistsAsync(
            crewName: "Crew A",
            date: today,
            morningMode: TransportMode.Driven,
            afternoonMode: TransportMode.Driven,
            baseRouteKm: 18,
            extraCommuteKm: 0,
            extraBusinessKm: 0,
            driverUsername: "rustam.driver",
            loggedByUsername: "bekzod.manager",
            notes: "Normal working day",
            crews,
            users,
            now);

        await CreateTransportDayIfNotExistsAsync(
            crewName: "Crew A",
            date: today.AddDays(1),
            morningMode: TransportMode.Taxi,
            afternoonMode: TransportMode.Driven,
            baseRouteKm: 18,
            extraCommuteKm: 2,
            extraBusinessKm: 5,
            driverUsername: "rustam.driver",
            loggedByUsername: "bekzod.manager",
            notes: "Morning taxi because vehicle problem",
            crews,
            users,
            now);

        await CreateTransportDayIfNotExistsAsync(
            crewName: "Crew B",
            date: today,
            morningMode: TransportMode.Driven,
            afternoonMode: TransportMode.Taxi,
            baseRouteKm: 25,
            extraCommuteKm: 0,
            extraBusinessKm: 10,
            driverUsername: "diyor.driver",
            loggedByUsername: "sardor.lead",
            notes: "Afternoon taxi",
            crews,
            users,
            now);

        await CreateTransportDayIfNotExistsAsync(
            crewName: "Crew C",
            date: today.AddDays(2),
            morningMode: TransportMode.None,
            afternoonMode: TransportMode.Driven,
            baseRouteKm: 15,
            extraCommuteKm: 3,
            extraBusinessKm: 7,
            driverUsername: "rustam.driver",
            loggedByUsername: "javlon.lead",
            notes: "Late start due to weather",
            crews,
            users,
            now);

        await _context.SaveChangesAsync();
    }

    private async Task CreateTransportDayIfNotExistsAsync(
        string crewName,
        DateOnly date,
        TransportMode morningMode,
        TransportMode? afternoonMode,
        double baseRouteKm,
        double extraCommuteKm,
        double extraBusinessKm,
        string driverUsername,
        string loggedByUsername,
        string notes,
        Dictionary<string, long> crews,
        Dictionary<string, long> users,
        DateTime now)
    {
        if (!crews.TryGetValue(crewName, out var crewId))
        {
            throw new InvalidOperationException(
                $"Crew '{crewName}' not found. " +
                "CrewDatabaseSeeder must run before TransportDayDatabaseSeeder.");
        }

        var exists = await _context.TransportDays
            .AnyAsync(x => x.CrewId == crewId && x.Date == date);

        if (exists)
            return;

        if (!users.TryGetValue(driverUsername, out var driverId))
            throw new InvalidOperationException($"Driver '{driverUsername}' not found.");
        
        if (!users.TryGetValue(loggedByUsername, out var loggedById))
            throw new InvalidOperationException($"User '{loggedByUsername}' not found.");
        
        var transportDay = new TransportDay
        {
            CrewId = crewId,
            Date = date,
            MorningMode = morningMode,
            AfternoonMode = afternoonMode,
            BaseRouteKm = baseRouteKm,
            ExtraCommuteKm = extraCommuteKm,
            ExtraBusinessKm = extraBusinessKm,
            DriverId = driverId,
            Notes = notes,
            LoggedBy = loggedById,
            LoggedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
        
        await _context.TransportDays.AddAsync(transportDay);
    }
}