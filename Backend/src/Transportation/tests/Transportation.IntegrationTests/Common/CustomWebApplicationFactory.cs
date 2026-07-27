using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Transportation.Infrastructure.Persistence;

namespace Transportation.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(TransportationDbContext) ||
                            d.ServiceType == typeof(DbContextOptions<TransportationDbContext>) ||
                            d.ServiceType.Name.Contains("DbContextOptions"))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            var initializerDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IHostedService) &&
                d.ImplementationType != null &&
                d.ImplementationType.Name.Contains("DatabaseInitializer"));

            if (initializerDescriptor is not null)
            {
                services.Remove(initializerDescriptor);
            }

            services.AddDbContext<TransportationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TransportationDb");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();

            dbContext.Database.EnsureCreated();
        });
    }
}