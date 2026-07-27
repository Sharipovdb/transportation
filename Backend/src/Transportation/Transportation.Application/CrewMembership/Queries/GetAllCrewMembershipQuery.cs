using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.CrewMembership.Queries;

public sealed record GetAllCrewMembershipQuery(
    long? CrewId,
    long? UserId,
    DateTime? ActiveFrom,
    DateTime? ActiveTo,
    PaginationInfo PaginationInfo
) : IQuery<PaginatedResult<CrewMembershipDto>>;

internal sealed class GetAllCrewMembershipQueryHandler : IQueryHandler<GetAllCrewMembershipQuery, PaginatedResult<CrewMembershipDto>>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly CrewMembershipMapper _mapper;

    public GetAllCrewMembershipQueryHandler(ICrewMembershipRepository crewMembershipRepository, CrewMembershipMapper mapper)
    {
        _crewMembershipRepository = crewMembershipRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CrewMembershipDto>> Handle(GetAllCrewMembershipQuery request, CancellationToken cancellationToken)
    {
        var countSpec = new GetAllCrewMembershipSpec(
            request.CrewId,
            request.UserId,
            request.ActiveFrom,
            request.ActiveTo
        );

        var listSpec = new GetAllCrewMembershipSpec(
            request.CrewId,
            request.UserId,
            request.ActiveFrom,
            request.ActiveTo,
            request.PaginationInfo
        );

        var totalCount = await _crewMembershipRepository.CountAsync(countSpec, cancellationToken);
        var entities = await _crewMembershipRepository.ListAsync(listSpec, cancellationToken);

        var mappedEntities = _mapper.Map(entities);

        return new PaginatedResult<CrewMembershipDto>(mappedEntities, totalCount);
    }
}