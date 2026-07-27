using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class CrewMembershipDatabaseSeeder : IDatabaseSeeder
{
    private readonly TransportationDbContext _context;

    public CrewMembershipDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.CrewMemberships.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var crewA = await _context.Crews.FirstAsync(c => c.Name == "Crew A");

        var crewB = await _context.Crews.FirstAsync(c => c.Name == "Crew B");

        var crewC = await _context.Crews.FirstAsync(c => c.Name == "Crew C");

        var workers = await _context.Users
            .Where(x => x.UserName!.StartsWith("worker."))
            .OrderBy(x => x.UserName)
            .ToListAsync();

        var memberships = new List<CrewMembership>
        {
            new()
            {
                CrewId = crewA.Id,
                UserId = workers[0].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewA.Id,
                UserId = workers[1].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewA.Id,
                UserId = workers[2].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewB.Id,
                UserId = workers[3].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewB.Id,
                UserId = workers[4].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewB.Id,
                UserId = workers[5].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewC.Id,
                UserId = workers[6].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewC.Id,
                UserId = workers[7].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewC.Id,
                UserId = workers[8].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                CrewId = crewC.Id,
                UserId = workers[9].Id,
                ActiveFrom = now,
                ActiveTo = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };

        await _context.CrewMemberships.AddRangeAsync(memberships);

        await _context.SaveChangesAsync();
    }
}