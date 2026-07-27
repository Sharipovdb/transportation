using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Transportation.Application.Common.Extensions;
using Transportation.Application.MonthlyTransportSheet.Calculations;
using Transportation.Application.MonthlyTransportSheet.Factories;
using Transportation.Application.MonthlyTransportSheet.Mappers;
using Transportation.Application.MonthlyTransportSheet.Services;
using Transportation.Application.PayoutLine;
using Transportation.Application.TransportDay;
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
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddScoped<PayoutLineMapperMinually>();

        services.TryAddSingleton<ICurrentUserAccessor, CurrentUserAccessor>();

        services.AddScoped<IMonthlyTransportSheetGenerator, MonthlyTransportSheetGenerator>();
        services.AddScoped<IMonthlyTransportCalculator, MonthlyTransportCalculator>();
        services.AddScoped<IMonthlyTransportSheetFactory, MonthlyTransportSheetFactory>();

        services.AddScoped<IMonthlyTransportPreviewMapper, MonthlyTransportPreviewMapper>();
        services.AddScoped<TransportDayMapper>();

        return services;
    }
}