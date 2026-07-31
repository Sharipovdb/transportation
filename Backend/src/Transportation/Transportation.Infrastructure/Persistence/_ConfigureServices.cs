using Transportation.Mediator.Helper.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transportation.Infrastructure.Persistence.Repositories;
using Transportation.Infrastructure.Persistence.Seeders;

namespace Transportation.Infrastructure.Persistence;

internal static class ConfigureServices
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddPersistenceServices(IConfiguration configuration, bool isDev)
        {
            services
                .AddDatabaseRepositories()
                .AddDbConnection(configuration, isDev)
                .AddSeeders();

            return services;
        }

        private IServiceCollection AddDbConnection(IConfiguration configuration, bool isDev)
        {
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TransportationDbContext>());

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<TransportationDbContext>(opt =>
                {
                    opt.UseNpgsql(connectionString, builder =>
                        {
                            builder.CommandTimeout(600);
                            builder.MigrationsAssembly(TransportationInfrastructureRef.Assembly.FullName);
                        }
                    );

                    if (isDev) opt.EnableSensitiveDataLogging();
                }
            );

            return services;
        }
    }
}