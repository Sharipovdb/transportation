using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class VehicleDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;
    
    public VehicleDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }
    
    public int Order => 6;

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        var users = await _context.Users
            .ToDictionaryAsync(x => x.UserName!, x => x.Id);
        
        await CreateVehicleIfNotExistsAsync(
            plate: "1234AB01",
            driverUsername: "rustam.driver",
            seatCount: 18,
            amortizationBasis: 50000m,
            users,
            now);
        
        await CreateVehicleIfNotExistsAsync(
            plate: "5678CD01",
            driverUsername: "diyor.driver",
            seatCount: 20,
            amortizationBasis: 65000m,
            users,
            now);
        
        await CreateVehicleIfNotExistsAsync(
            plate: "9012EF01",
            driverUsername: "rustam.driver",
            seatCount: 16,
            amortizationBasis: 45000m,
            users,
            now);
        
        await _context.SaveChangesAsync();
    }
    
    private async Task CreateVehicleIfNotExistsAsync(
        string plate,
        string driverUsername,
        int seatCount,
        decimal amortizationBasis,
        Dictionary<string,long> users,
        DateTime now)
    {
        if(await _context.Vehicles.AnyAsync())
            return;
        
        if (!users.TryGetValue(driverUsername, out var driverId))
        {
            throw new InvalidOperationException(
                $"Driver '{driverUsername}' not found. " +
                "UserDatabaseSeeder must run before VehicleDatabaseSeeder.");
        }

        var vehicle = new Vehicle
        {
            DriverId = driverId,
            Plate = plate,
            SeatCount = seatCount,
            AmortizationBasis = amortizationBasis,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
        
        await _context.Vehicles.AddAsync(vehicle);
    }
}