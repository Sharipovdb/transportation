using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Transportation.Infrastructure.Persistence;

public class TransportationDbContextFactory : IDesignTimeDbContextFactory<TransportationDbContext>
{
    public TransportationDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Transportation.API");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<TransportationDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new TransportationDbContext(optionsBuilder.Options);
    }
}