using Microsoft.Extensions.DependencyInjection;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal static class ConfigureServices
{
    internal static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        services
            .AddHostedService<DatabaseInitializer>()
            .AddScoped<IDatabaseSeeder, RouteDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, UserDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, RoleDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, UserRoleDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, CrewDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, CrewMembershipDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, VehicleDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, AbsenceNoticeDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, TransportDayDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, TaxiExpenseDatabaseSeeder>()
            .AddScoped<IDatabaseSeeder, TransportSettingsDatabaseSeeder>();

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}