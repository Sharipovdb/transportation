using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class DatabaseSeeder
{
    private readonly IEnumerable<IDatabaseSeeder> _seeders;

    public DatabaseSeeder(IEnumerable<IDatabaseSeeder> seeders)
    {
        _seeders = seeders;
    }

    public async Task SeedAsync()
    {
        foreach (var seeder in _seeders)
        {
            await seeder.SeedAsync();
        }
    }
}