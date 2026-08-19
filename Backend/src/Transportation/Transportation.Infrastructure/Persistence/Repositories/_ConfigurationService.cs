using Microsoft.Extensions.DependencyInjection;
using Transportation.Application.AbsenceNotice.Repositories;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.Route.Repositories;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.Vehicle.Repositories;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.TransportSettings.Repositories;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal static class ConfigureServices
{
    internal static IServiceCollection AddDatabaseRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMonthlyTransportSheetRepository, MonthlyTransportSheetRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<ITransportDayRepository, TransportDayRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IAbsenceNoticeRepository, AbsenceNoticeRepository>();
        services.AddScoped<ITaxiExpenseRepository, TaxiExpenseRepository>();
        services.AddScoped<ICrewRepository, CrewRepository>();
        services.AddScoped<ICrewMembershipRepository, CrewMemborshipRepository>();
        services.AddScoped<ITransportSettingsRepository, TransportSettingsRepository>();

        return services;
    }
}