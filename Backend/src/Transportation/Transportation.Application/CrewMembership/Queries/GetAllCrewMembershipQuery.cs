using Ardalis.Specification;
using MediatR;
using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.CrewMembership.Queries;

public sealed record GetAllCrewMembershipQuery(
    long? CrewId,
    long? UserId,
    DateTime? ActiveFrom,
    DateTime? ActiveTo,
    PaginationInfo PaginationInfo
) : IQuery<PaginatedResult<CrewMembershipDto>>;

internal sealed class GetAllCrewMembershipQueryHandler
    : IRequestHandler<GetAllCrewMembershipQuery, PaginatedResult<CrewMembershipDto>>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly CrewMembershipMapper _mapper;

    public GetAllCrewMembershipQueryHandler(ICrewMembershipRepository crewMembershipRepository,
        CrewMembershipMapper mapper)
    {
        _crewMembershipRepository = crewMembershipRepository;
        _mapper = mapper;
    }


    public async Task<PaginatedResult<CrewMembershipDto>> Handle(GetAllCrewMembershipQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.CrewMembership>();
        if (request.CrewId.HasValue)
            spec.Query.Where(x => x.CrewId == request.CrewId);

        if (request.UserId.HasValue)
            spec.Query.Where(x => x.UserId == request.UserId);

        if (request.ActiveFrom.HasValue)
            spec.Query.Where(x => x.ActiveFrom == request.ActiveFrom);

        if (request.ActiveTo.HasValue)
            spec.Query.Where(x => x.ActiveTo == request.ActiveTo);

        spec.Query
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Id)
            .WithPagination(request.PaginationInfo);

        var entities = await _crewMembershipRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _crewMembershipRepository.CountAsync(spec, cancellationToken);

        var mappedEntity = _mapper.Map(entities);

        return new PaginatedResult<CrewMembershipDto>(mappedEntity, totalCount);
    }
}