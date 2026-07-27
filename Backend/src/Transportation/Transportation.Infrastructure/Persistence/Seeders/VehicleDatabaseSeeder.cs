using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class VehicleDatabaseSeeder : IDatabaseSeeder
{
    private readonly TransportationDbContext _context;

    public VehicleDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }


    public async Task SeedAsync()
    {
        if (await _context.Vehicles.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var rustam = await _context.Users.FirstAsync(x => x.UserName == "rustam.driver");

        var diyor = await _context.Users.FirstAsync(x => x.UserName == "diyor.driver");

        var vehicles = new List<Vehicle>
        {
            new()
            {
                DriverId = rustam.Id,
                Plate = "1234AB01",
                SeatCount = 18,
                AmortizationBasis = 50000m,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                DriverId = diyor.Id,
                Plate = "5678CD01",
                SeatCount = 20,
                AmortizationBasis = 65000m,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                DriverId = rustam.Id,
                Plate = "9012EF01",
                SeatCount = 16,
                AmortizationBasis = 45000m,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };

        await _context.Vehicles.AddRangeAsync(vehicles);

        await _context.SaveChangesAsync();
    }
}