using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class RouteDatabaseSeeder : IDatabaseSeeder
{
    private readonly TransportationDbContext _context;

    public RouteDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var isAnyRoute = await _context.Routes.AnyAsync();

        if (isAnyRoute)
            return;

        var now = DateTime.UtcNow;

        var routes = new List<Route>
        {
            new()
            {
                Name = "Khujand (Panjshanbe) → Dehmoy",
                DistanceKm = 18,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Khujand (Univermag) → Dehmoy",
                DistanceKm = 17,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Khujand (8th Microdistrict) → Dehmoy",
                DistanceKm = 16,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Khujand (20th Microdistrict) → Dehmoy",
                DistanceKm = 19,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Khujand (34th Microdistrict) → Dehmoy",
                DistanceKm = 20,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Khujand (Somon Bazaar) → Dehmoy",
                DistanceKm = 18,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Khujand (Airport) → Dehmoy",
                DistanceKm = 24,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Buston → Dehmoy",
                DistanceKm = 10,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Ghafurov → Dehmoy",
                DistanceKm = 8,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Jabbor Rasulov → Dehmoy",
                DistanceKm = 6,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Mehrobod → Dehmoy",
                DistanceKm = 4,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            new()
            {
                Name = "Yova → Dehmoy",
                DistanceKm = 13,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };

        await _context.Routes.AddRangeAsync(routes);
        await _context.SaveChangesAsync();
    }
}