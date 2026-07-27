using Transportation.Application.Route.Models;
using Transportation.Application.Route.Repositories;
using Transportation.Application.Route.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.Route.Queries;

public sealed record GetRouteById(long Id) : IQuery<RouteDto>;

internal sealed class GetRouteByIdHandler : IQueryHandler<GetRouteById, RouteDto>
{
    private readonly IRouteRepository _routeRepository;
    private readonly RouteMapper _routeMapper;

    public GetRouteByIdHandler(IRouteRepository routeRepository, RouteMapper routeMapper)
    {
        _routeRepository = routeRepository;
        _routeMapper = routeMapper;
    }

    public async Task<RouteDto> Handle(GetRouteById request, CancellationToken cancellationToken)
    {
        var entity = await _routeRepository.FirstOrDefaultAsync(new RouteByIdSpec(request.Id), cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(RouteError.NotFound);

        return _routeMapper.Map(entity);
    }
}