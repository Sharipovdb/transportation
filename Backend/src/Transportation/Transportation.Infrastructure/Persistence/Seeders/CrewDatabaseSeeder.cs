using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class CrewDatabaseSeeder : IDatabaseSeeder
{
    private readonly TransportationDbContext _context;

    public CrewDatabaseSeeder(
        TransportationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Crews.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var panjshanbeRoute = await _context.Routes.FirstAsync(r => r.Name == "Khujand (Panjshanbe) → Dehmoy");

        var univermagRoute = await _context.Routes.FirstAsync(r => r.Name == "Khujand (Univermag) → Dehmoy");

        var micro8Route = await _context.Routes.FirstAsync(r => r.Name == "Khujand (8th Microdistrict) → Dehmoy");

        var sardor = await _context.Users.FirstAsync(x => x.UserName == "sardor.lead");

        var javlon = await _context.Users.FirstAsync(x => x.UserName == "javlon.lead");

        var rustam = await _context.Users.FirstAsync(x => x.UserName == "rustam.driver");

        var diyor = await _context.Users.FirstAsync(x => x.UserName == "diyor.driver");

        var crews = new List<Crew>
        {
            new()
            {
                Name = "Crew A",
                RouteId = panjshanbeRoute.Id,
                CrewLeadId = sardor.Id,
                DriverLeadId = rustam.Id,
                SeatCapacity = 12,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                Name = "Crew B",
                RouteId = univermagRoute.Id,
                CrewLeadId = javlon.Id,
                DriverLeadId = diyor.Id,
                SeatCapacity = 14,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                Name = "Crew C",
                RouteId = micro8Route.Id,
                CrewLeadId = sardor.Id,
                DriverLeadId = rustam.Id,
                SeatCapacity = 10,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };

        await _context.Crews.AddRangeAsync(crews);

        await _context.SaveChangesAsync();
    }
}