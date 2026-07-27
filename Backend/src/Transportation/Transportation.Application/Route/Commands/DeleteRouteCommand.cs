using Transportation.Application.Route.Repositories;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Route.Commands;

public record DeleteRouteCommand(long Id) : ICommand<bool>;

internal sealed class DeleteRouteCommandHandler(
    IRouteRepository routeRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : ICommandHandler<DeleteRouteCommand, bool>
{
    public async Task<bool> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await routeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (route is null || route.IsDeleted)
            throw new BusinessLogicException(RouteError.NotFound);

        route.IsDeleted = true;
        route.UpdatedAt = timeProvider.GetLocalDateTimeNowKindUtc();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}