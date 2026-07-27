using Ardalis.Specification;
using Transportation.Application.Crew.Models;
using Transportation.Application.Crew.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

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
        var spec = new ReadOnlySpecification<Domain.Entities.Crew>();

        if (request.Name is not null)
            spec.Query.Where(x => x.Name.Contains(request.Name));

        if (request.RouteId.HasValue)
            spec.Query.Where(x => x.RouteId == request.RouteId);

        if (request.CrewLeadId.HasValue)
            spec.Query.Where(x => x.CrewLeadId == request.CrewLeadId);

        if (request.DriverLeadId.HasValue)
            spec.Query.Where(x => x.DriverLeadId == request.DriverLeadId);

        if (request.SeatCapacity.HasValue)
            spec.Query.Where(x => x.SeatCapacity == request.SeatCapacity);

        spec.Query
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .WithPagination(request.PaginationInfo);

        var entities = await _crewRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _crewRepository.CountAsync(spec, cancellationToken);

        var mappedEntity = _mapper.Map(entities);

        return new PaginatedResult<CrewDto>(mappedEntity, totalCount);
    }
}