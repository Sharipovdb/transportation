using FluentValidation;
using Transportation.Application.Route.Models;
using Transportation.Application.Route.Repositories;
using Transportation.Application.Route.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Route.Commands;

public record CreateRouteCommand(
    string Name,
    double DistanceKm
) : ICommand<RouteDto>;

// ReSharper disable once UnusedType.Global
public sealed class CreateRouteCommandValidator : AbstractValidator<CreateRouteCommand>
{
    public CreateRouteCommandValidator()
    {
        RuleFor(e => e.Name)
            .MinimumLength(3)
            .MaximumLength(50);
    }
}

internal sealed class CreateRouteCommandHandler(
    IRouteRepository routeRepository,
    IUnitOfWork unitOfWork,
    RouteMapper routeMapper,
    TimeProvider timeProvider
) : ICommandHandler<CreateRouteCommand, RouteDto>
{
    public async Task<RouteDto> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        var spec = new RouteByNameSpec(request.Name);
        var result = await routeRepository.AnyAsync(spec, cancellationToken);

        if (result)
            throw new BusinessLogicException(RouteError.AlreadyExist);

        var route = new Domain.Entities.Route
        {
            Name = request.Name,
            DistanceKm = request.DistanceKm,
            CreatedAt = timeProvider.GetLocalDateTimeNowKindUtc()
        };

        await routeRepository.AddAsync(route, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return routeMapper.Map(route);
    }
}