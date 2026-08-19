using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence;

internal sealed class DatabaseInitializer : IHostedService
{
    /// <summary>
    /// Opt-in switch for the demo dataset. Off unless the configuration says otherwise.
    /// </summary>
    private const string SeedDemoDataKey = "SeedDemoData";

    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public DatabaseInitializer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var blankSeeders = scope.ServiceProvider
            .GetServices<IBlankDataSeeder>()
            .OrderBy(x => x.Order);

        foreach (var seeder in blankSeeders)
        {
            await seeder.SeedAsync();
        }

        // Demo data used to go in on every Development start. Invented crews, transport
        // days and taxi fares are indistinguishable from real ones once they are in the
        // database, and they end up on payout sheets the company acts on — so seeding
        // them now has to be asked for explicitly, per environment.
        if (!_configuration.GetValue<bool>(SeedDemoDataKey))
            return;

        var demoSeeders = scope.ServiceProvider
            .GetServices<IDemoDataSeeder>()
            .OrderBy(x => x.Order);

        foreach (var seeder in demoSeeders)
        {
            await seeder.SeedAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
