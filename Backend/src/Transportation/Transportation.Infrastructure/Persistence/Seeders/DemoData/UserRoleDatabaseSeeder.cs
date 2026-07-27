using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;
using Transportation.Shared.Authorization;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class UserRoleDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public UserRoleDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 3;

    public async Task SeedAsync()
    {
        var users = await _context.Users
            .ToDictionaryAsync(x => x.UserName!, x => x.Id);

        var roles = await _context.Roles
            .ToDictionaryAsync(x => x.Name!, x => x.Id);

        var mappings = new Dictionary<string, string>
        {
            ["bekzod.manager"] = RoleNames.RouteManager,
            ["sardor.lead"] = RoleNames.CrewLead,
            ["javlon.lead"] = RoleNames.CrewLead,
            ["rustam.driver"] = RoleNames.DriverLead,
            ["diyor.driver"] = RoleNames.DriverLead,
            ["worker.01"] = RoleNames.Worker,
            ["worker.02"] = RoleNames.Worker,
            ["worker.03"] = RoleNames.Worker,
            ["worker.04"] = RoleNames.Worker,
            ["worker.05"] = RoleNames.Worker,
            ["worker.06"] = RoleNames.Worker,
            ["worker.07"] = RoleNames.Worker,
            ["worker.08"] = RoleNames.Worker,
            ["worker.09"] = RoleNames.Worker,
            ["worker.10"] = RoleNames.Worker,
            ["accountant"] = RoleNames.Accountant
        };

        foreach (var item in mappings)
        {
            if (!users.TryGetValue(item.Key, out var userId))
                continue;

            if (!roles.TryGetValue(item.Value, out var roleId))
                continue;

            var exists = await _context.UserRoles
                .AnyAsync(x => x.UserId == userId && x.RoleId == roleId);

            if (exists)
                continue;

            await _context.UserRoles.AddAsync(
                new IdentityUserRole<long> { UserId = userId, RoleId = roleId });
        }

        await _context.SaveChangesAsync();
    }
}