using Microsoft.Extensions.DependencyInjection;
using Transportation.Infrastructure.Persistence;

namespace Transportation.IntegrationTests.Common;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly CustomWebApplicationFactory Factory;
    
    public IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
    }
    
    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
}