using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class TransportSettingsDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public TransportSettingsDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 100;

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        var settings = new[]
        {
            new
            {
                CommuteKmRate = 2.50m,
                ExtraBusinessKmRate = 4.00m,
                EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                CommuteKmRate = 3.00m,
                ExtraBusinessKmRate = 5.00m,
                EffectiveFrom = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        foreach (var item in settings)
        {
            var exists = await _context.TransportSettings
                .AnyAsync(x => x.EffectiveFrom == item.EffectiveFrom);

            if (exists)
                continue;

            var entity = new TransportSettings
            {
                CommuteKmRate = item.CommuteKmRate,
                ExtraBusinessKmRate = item.ExtraBusinessKmRate,
                EffectiveFrom = item.EffectiveFrom,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            await _context.TransportSettings.AddAsync(entity);
        }

        await _context.SaveChangesAsync();
    }
}