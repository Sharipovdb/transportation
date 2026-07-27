using Ardalis.Specification;
using Transportation.Application.Route.Models;
using Transportation.Application.Route.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.Route.Queries;

public sealed record GetAllRoutes(PaginationInfo PaginationInfo) : IQuery<PaginatedResult<RouteDto>>;

public sealed class GetAllRouteHandler : IQueryHandler<GetAllRoutes, PaginatedResult<RouteDto>>
{
    private readonly IRouteRepository _routeRepository;
    private readonly RouteMapper _mapper;

    public GetAllRouteHandler(IRouteRepository routeRepository, RouteMapper mapper)
    {
        _routeRepository = routeRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<RouteDto>> Handle(GetAllRoutes request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.Route>();

        spec.Query
            .Where(x => !x.IsDeleted)
            .WithPagination(request.PaginationInfo);

        var routes = await _routeRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _routeRepository.CountAsync(spec, cancellationToken);

        if (totalCount == 0)
            throw new ResourceNotFoundException(RouteError.NotFound);

        var mappedRoutes = _mapper.Map(routes);

        return new PaginatedResult<RouteDto>(mappedRoutes, totalCount);
    }
}