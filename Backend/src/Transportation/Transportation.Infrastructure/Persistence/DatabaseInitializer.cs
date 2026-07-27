using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence;

internal sealed class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public DatabaseInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
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
        
        if (_environment.IsDevelopment())
        {
            var demoSeeders = scope.ServiceProvider
                .GetServices<IDemoDataSeeder>()
                .OrderBy(x => x.Order);
            
            foreach (var seeder in demoSeeders)
            {
                await seeder.SeedAsync();
            }
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}