using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Transportation.Application.Common.Extensions;
using Transportation.Application.MonthlyTransportSheet.Services;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Services;
using Transportation.Mediator.Helper.Behaviors;
using Transportation.Shared;

namespace Transportation.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddMappers();

        services.AddValidatorsFromAssembly(ApplicationRef.Assembly);

        services.AddMediatR(x => x.RegisterServicesFromAssembly(ApplicationRef.Assembly));

        services
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.TryAddSingleton<ICurrentUserAccessor, CurrentUserAccessor>();

        services.AddScoped<IMonthlyTransportSheetBuilder, MonthlyTransportSheetBuilder>();
        services.AddScoped<ITransportDayTaxiFareService, TransportDayTaxiFareService>();
        services.AddScoped<TransportDayMapper>();

        return services;
    }
}
