using Microsoft.AspNetCore.Identity;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;
using Transportation.Shared.Authorization;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class UserRoleDatabaseSeeder : IDatabaseSeeder
{
    private readonly UserManager<User> _userManager;

    public UserRoleDatabaseSeeder(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        var assignments = new (string UserName, string RoleName)[]
        {
            ("admin", RoleNames.Admin),
            ("bekzod.manager", RoleNames.RouteManager),
            ("sardor.lead", RoleNames.CrewLead),
            ("javlon.lead", RoleNames.CrewLead),
            ("rustam.driver", RoleNames.DriverLead),
            ("diyor.driver", RoleNames.DriverLead),
            ("worker.01", RoleNames.Worker),
            ("worker.02", RoleNames.Worker),
            ("worker.03", RoleNames.Worker),
            ("worker.04", RoleNames.Worker),
            ("worker.05", RoleNames.Worker),
            ("worker.06", RoleNames.Worker),
            ("worker.07", RoleNames.Worker),
            ("worker.08", RoleNames.Worker),
            ("worker.09", RoleNames.Worker),
            ("worker.10", RoleNames.Worker),
            ("accountant", RoleNames.Accountant),
        };

        foreach (var (userName, roleName) in assignments)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user is null)
                continue;

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
