using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class CrewDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public CrewDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 4;

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        var routes = await _context.Routes
            .ToDictionaryAsync(x => x.Name, x => x.Id);

        var users = await _context.Users
            .ToDictionaryAsync(x => x.UserName!, x => x.Id);

        await CreateCrewIfNotExistsAsync(
            crewName: "Crew A",
            routeName: "Khujand (Panjshanbe) → Dehmoy",
            crewLeadUsername: "sardor.lead",
            driverLeadUsername: null,
            seatCapacity: 12,
            routes,
            users,
            now);

        await CreateCrewIfNotExistsAsync(
            crewName: "Crew B",
            routeName: "Khujand (Univermag) → Dehmoy",
            crewLeadUsername: null,
            driverLeadUsername: "rustam.driver",
            seatCapacity: 14,
            routes,
            users,
            now);

        await CreateCrewIfNotExistsAsync(
            crewName: "Crew C",
            routeName: "Khujand (8th Microdistrict) → Dehmoy",
            crewLeadUsername: "javlon.lead",
            driverLeadUsername: null,
            seatCapacity: 10,
            routes,
            users,
            now);

        await _context.SaveChangesAsync();
    }

    private async Task CreateCrewIfNotExistsAsync(
        string crewName,
        string routeName,
        string? crewLeadUsername,
        string? driverLeadUsername,
        int seatCapacity,
        Dictionary<string, long> routes,
        Dictionary<string, long> users,
        DateTime now)
    {
        if (await _context.Vehicles.AnyAsync())
            return;

        if (!routes.TryGetValue(routeName, out var routeId))
        {
            throw new InvalidOperationException(
                $"Route '{routeName}' not found. " +
                "RouteDatabaseSeeder must run before CrewDatabaseSeeder.");
        }

        long? crewLeadId = null;
        long? driverLeadId = null;

        if (crewLeadUsername is not null)
        {
            if (!users.TryGetValue(crewLeadUsername, out var userId))
            {
                throw new InvalidOperationException(
                    $"CrewLead '{crewLeadUsername}' not found. " +
                    "UserDatabaseSeeder must run before CrewDatabaseSeeder.");
            }

            crewLeadId = userId;
        }


        if (driverLeadUsername is not null)
        {
            if (!users.TryGetValue(driverLeadUsername, out var userId))
            {
                throw new InvalidOperationException(
                    $"DriverLead '{driverLeadUsername}' not found. " +
                    "UserDatabaseSeeder must run before CrewDatabaseSeeder.");
            }

            driverLeadId = userId;
        }

        if (crewLeadId is not null && driverLeadId is not null)
            throw new InvalidOperationException("Crew cannot have both CrewLead and DriverLead.");


        if (crewLeadId is null && driverLeadId is null)
            throw new InvalidOperationException("Crew must have either CrewLead or DriverLead.");

        var crew = new Crew
        {
            Name = crewName,
            RouteId = routeId,
            CrewLeadId = crewLeadId,
            DriverLeadId = driverLeadId,
            SeatCapacity = seatCapacity,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        await _context.Crews.AddAsync(crew);
    }
}