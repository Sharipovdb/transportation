using FluentValidation;
using Transportation.Application.Route.Repositories;
using Transportation.Application.Route.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Route.Commands;

public record UpdateRouteCommand(
    long Id,
    string Name,
    double DistanceKm
) : ICommand;

// ReSharper disable once UnusedType.Global
public sealed class UpdateRouteCommandValidator : AbstractValidator<UpdateRouteCommand>
{
    public UpdateRouteCommandValidator()
    {
        RuleFor(e => e.Name)
            .MinimumLength(3)
            .MaximumLength(50);
    }
}

internal sealed class UpdateRouteCommandHandler(
    IRouteRepository routeRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : ICommandHandler<UpdateRouteCommand>
{
    public async Task Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await routeRepository.FirstOrDefaultAsync(new RouteByIdSpec(request.Id), cancellationToken);

        if (route is null)
            throw new ResourceNotFoundException(RouteError.NotFound);


        route.Name = request.Name;
        route.DistanceKm = request.DistanceKm;
        route.UpdatedAt = timeProvider.GetLocalDateTimeNowKindUtc();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}