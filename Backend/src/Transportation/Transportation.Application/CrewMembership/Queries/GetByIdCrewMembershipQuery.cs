using Transportation.Application.CrewMembership.Models;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.CrewMembership.Queries;

public sealed record GetByIdCrewMembershipQuery(long Id) : IQuery<CrewMembershipDto>;

internal sealed class GetByIdCrewMembershipQueryHandler : IQueryHandler<GetByIdCrewMembershipQuery, CrewMembershipDto>
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly CrewMembershipMapper _mapper;

    public GetByIdCrewMembershipQueryHandler(ICrewMembershipRepository crewMembershipRepository,
        CrewMembershipMapper mapper)
    {
        _crewMembershipRepository = crewMembershipRepository;
        _mapper = mapper;
    }

    public async Task<CrewMembershipDto> Handle(GetByIdCrewMembershipQuery request, CancellationToken cancellationToken)
    {
        var spec = new CrewMembershipByIdSpec(request.Id);
        var entity = await _crewMembershipRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewMembershipErrors.NotFound);

        return _mapper.Map(entity);
    }
}