using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class RouteDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public RouteDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 1;

    public async Task SeedAsync()
    {
        var routes = new[]
        {
            new
            {
                Name = "Khujand (Panjshanbe) → Dehmoy",
                DistanceKm = 18
            },
            new
            {
                Name = "Khujand (Univermag) → Dehmoy",
                DistanceKm = 17
            },
            new
            {
                Name = "Khujand (8th Microdistrict) → Dehmoy",
                DistanceKm = 16
            },
            new
            {
                Name = "Khujand (20th Microdistrict) → Dehmoy",
                DistanceKm = 19
            },
            new
            {
                Name = "Khujand (34th Microdistrict) → Dehmoy",
                DistanceKm = 20
            },
            new
            {
                Name = "Khujand (Somon Bazaar) → Dehmoy",
                DistanceKm = 18
            },
            new
            {
                Name = "Khujand (Airport) → Dehmoy",
                DistanceKm = 24
            },
            new
            {
                Name = "Buston → Dehmoy",
                DistanceKm = 10
            },
            new
            {
                Name = "Ghafurov → Dehmoy",
                DistanceKm = 8
            },
            new
            {
                Name = "Jabbor Rasulov → Dehmoy",
                DistanceKm = 6
            },
            new
            {
                Name = "Mehrobod → Dehmoy",
                DistanceKm = 4
            },
            new
            {
                Name = "Yova → Dehmoy",
                DistanceKm = 13
            }
        };

        var now = DateTime.UtcNow;

        foreach (var item in routes)
        {
            await CreateRouteIfNotExistsAsync(item.Name, item.DistanceKm, now);
        }

        await _context.SaveChangesAsync();
    }


    private async Task CreateRouteIfNotExistsAsync(string name, double distanceKm, DateTime now)
    {
        var exists = await _context.Routes .AnyAsync(x => x.Name == name);
        
        if (exists)
            return;

        var route = new Route
        {
            Name = name,
            DistanceKm = distanceKm,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
        
        await _context.Routes.AddAsync(route);
    }
}