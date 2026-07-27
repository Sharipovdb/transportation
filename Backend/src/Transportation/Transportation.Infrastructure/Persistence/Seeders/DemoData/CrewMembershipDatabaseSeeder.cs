using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class CrewMembershipDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public CrewMembershipDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 5;
    
    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;
        
        var crews = await _context.Crews
            .ToDictionaryAsync(x => x.Name, x => x.Id);
        
        var users = await _context.Users
            .ToDictionaryAsync(x => x.UserName!, x => x.Id);

        var memberships = new[]
        {
            new
            {
                Crew = "Crew A",
                User = "worker.01"
            },
            new
            {
                Crew = "Crew A",
                User = "worker.02"
            },
            new
            {
                Crew = "Crew A",
                User = "worker.03"
            },
            new
            {
                Crew = "Crew B",
                User = "worker.04"
            },
            new
            {
                Crew = "Crew B",
                User = "worker.05"
            },
            new
            {
                Crew = "Crew B",
                User = "worker.06"
            },
            new
            {
                Crew = "Crew C",
                User = "worker.07"
            },
            new
            {
                Crew = "Crew C",
                User = "worker.08"
            },
            new
            {
                Crew = "Crew C",
                User = "worker.09"
            },
            new
            {
                Crew = "Crew C",
                User = "worker.10"
            }
        };
        
        foreach (var item in memberships)
        {
            await CreateMembershipIfNotExistsAsync(item.Crew, item.User, crews, users, now);
        }

        await _context.SaveChangesAsync();
    }
    
    private async Task CreateMembershipIfNotExistsAsync(
        string crewName,
        string username,
        Dictionary<string,long> crews,
        Dictionary<string,long> users,
        DateTime now)
    {
        if (!crews.TryGetValue( crewName, out var crewId))
        {
            throw new InvalidOperationException(
                $"Crew '{crewName}' not found. " +
                "CrewDatabaseSeeder must run before CrewMembershipDatabaseSeeder.");
        }
        
        if (!users.TryGetValue( username, out var userId))
        {
            throw new InvalidOperationException(
                $"User '{username}' not found. " +
                "UserDatabaseSeeder must run before CrewMembershipDatabaseSeeder.");
        }

        var exists = await _context.CrewMemberships
            .AnyAsync(x => x.CrewId == crewId && x.UserId == userId && x.IsActive);
        
        if (exists)
            return;
        
        var membership = new CrewMembership
        {
            CrewId = crewId,
            UserId = userId,
            ActiveFrom = now,
            ActiveTo = null,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        await _context.CrewMemberships.AddAsync(membership);
    }
}