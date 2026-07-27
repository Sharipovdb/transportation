using Microsoft.AspNetCore.Identity;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;
using Transportation.Shared.Authorization;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class RoleDatabaseSeeder : IDatabaseSeeder
{
    private readonly RoleManager<IdentityRole<long>> _roleManager;

    public RoleDatabaseSeeder(RoleManager<IdentityRole<long>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        var roles = new[]
        {
            RoleNames.Admin,
            RoleNames.RouteManager,
            RoleNames.CrewLead,
            RoleNames.DriverLead,
            RoleNames.Worker,
            RoleNames.Accountant
        };

        foreach (var roleName in roles)
        {
            var exists = await _roleManager.RoleExistsAsync(roleName);

            if (exists)
                continue;

            var role = new IdentityRole<long>
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));

                throw new Exception($"Role '{roleName}' creation failed. Errors: {errors}");
            }
        }
    }
}