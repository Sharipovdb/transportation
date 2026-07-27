using Microsoft.AspNetCore.Identity;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;
using Transportation.Shared.Authorization;

namespace Transportation.Infrastructure.Persistence.Seeders.BlankData;

internal sealed class RoleDatabaseSeeder : IBlankDataSeeder
{
    private readonly RoleManager<IdentityRole<long>> _roleManager;

    public RoleDatabaseSeeder(RoleManager<IdentityRole<long>> roleManager)
    {
        _roleManager = roleManager;
    }

    public int Order => 1;

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
            await CreateRoleIfNotExistsAsync(roleName);
        }
    }

    private async Task CreateRoleIfNotExistsAsync(string roleName)
    {
        var exists = await _roleManager.RoleExistsAsync(roleName);

        if (exists)
            return;

        var role = new IdentityRole<long>
        {
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant()
        };
        
        var result = await _roleManager.CreateAsync(role);
        
        if (result.Succeeded)
            return;
        
        var errors = string.Join(", ", result.Errors.Select(x => x.Description));

        throw new InvalidOperationException($"Failed to create role '{roleName}'. Errors: {errors}");
    }
}