using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Repositories;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Queries;
using Transportation.Application.Crew.Specification;

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
    private readonly CrewMapper _mapper;

    public GetAllCrewQueryHandler(ICrewRepository crewRepository, CrewMapper mapper)
    {
        _crewRepository = crewRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CrewDto>> Handle(GetAllCrewQuery request, CancellationToken cancellationToken)
    {
        var countSpec = new GetAllCrewSpec(
            request.Name,
            request.RouteId,
            request.CrewLeadId,
            request.DriverLeadId,
            request.SeatCapacity
        );

        var listSpec = new GetAllCrewSpec(
            request.Name,
            request.RouteId,
            request.CrewLeadId,
            request.DriverLeadId,
            request.SeatCapacity,
            request.PaginationInfo
        );

        var totalCount = await _crewRepository.CountAsync(countSpec, cancellationToken);
        var entities = await _crewRepository.ListAsync(listSpec, cancellationToken);

        var mappedEntities = _mapper.Map(entities);

        return new PaginatedResult<CrewDto>(mappedEntities, totalCount);
    }
}