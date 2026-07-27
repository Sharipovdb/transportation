using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class TransportSettingsDatabaseSeeder : IDatabaseSeeder
{
    private readonly TransportationDbContext _context;

    public TransportSettingsDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.TransportSettings.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var settings = new List<TransportSettings>
        {
            new()
            {
                CommuteKmRate = 2.50m,
                ExtraBusinessKmRate = 4.00m,
                EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CommuteKmRate = 3.00m,
                ExtraBusinessKmRate = 5.00m,
                EffectiveFrom = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };
        
        await _context.TransportSettings.AddRangeAsync(settings);

        await _context.SaveChangesAsync();
    }
}