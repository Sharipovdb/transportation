using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Repositories;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Queries;
using Transportation.Application.Crew.Specification;
using Transportation.Application.Crew.Services;

namespace Transportation.Application.Crew.Queries;

public sealed record GetAllCrewQuery(
    string? Name,
    long? RouteId,
    long? CrewLeadId,
    long? DriverLeadId,
    int? SeatCapacity,
    PaginationInfo PaginationInfo
) : IQuery<PaginatedResult<CrewDto>>;

internal sealed class GetAllCrewQueryHandler : IQueryHandler<GetAllCrewQuery, PaginatedResult<CrewDto>>
{
    private readonly ICrewRepository _crewRepository;
    private readonly ICrewVisibility _crewVisibility;
    private readonly CrewMapper _mapper;

    public GetAllCrewQueryHandler(
        ICrewRepository crewRepository,
        ICrewVisibility crewVisibility,
        CrewMapper mapper)
    {
        _crewRepository = crewRepository;
        _crewVisibility = crewVisibility;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CrewDto>> Handle(GetAllCrewQuery request, CancellationToken cancellationToken)
    {
        // The count and the page are two specs over the same set, so the scope reaches
        // both — otherwise the list would report more crews than it can show.
        var visibleCrewIds = await _crewVisibility.VisibleCrewIdsAsync(cancellationToken);

        var countSpec = new GetAllCrewSpec(
            request.Name,
            request.RouteId,
            request.CrewLeadId,
            request.DriverLeadId,
            request.SeatCapacity,
            visibleCrewIds: visibleCrewIds
        );

        var listSpec = new GetAllCrewSpec(
            request.Name,
            request.RouteId,
            request.CrewLeadId,
            request.DriverLeadId,
            request.SeatCapacity,
            request.PaginationInfo,
            visibleCrewIds
        );

        var totalCount = await _crewRepository.CountAsync(countSpec, cancellationToken);
        var entities = await _crewRepository.ListAsync(listSpec, cancellationToken);

        var mappedEntities = _mapper.Map(entities);

        return new PaginatedResult<CrewDto>(mappedEntities, totalCount);
    }
}