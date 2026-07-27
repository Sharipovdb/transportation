using Microsoft.Extensions.DependencyInjection;
using Transportation.Infrastructure.Persistence.Seeders.BlankData;
using Transportation.Infrastructure.Persistence.Seeders.DemoData;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal static class ConfigureServices
{
    internal static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        services.AddHostedService<DatabaseInitializer>();

        services
            .AddScoped<IBlankDataSeeder, RoleDatabaseSeeder>()
            .AddScoped<IBlankDataSeeder, AdminDatabaseSeeder>();
        
        services
            .AddScoped<IDemoDataSeeder, RouteDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, UserDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, UserRoleDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, CrewDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, CrewMembershipDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, VehicleDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, TransportSettingsDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, AbsenceNoticeDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, TransportDayDatabaseSeeder>()
            .AddScoped<IDemoDataSeeder, TaxiExpenseDatabaseSeeder>();

        return services;
    }
}