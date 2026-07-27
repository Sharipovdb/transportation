using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class ActiveCrewMembershipsByCrewIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public ActiveCrewMembershipsByCrewIdSpec(long crewId, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.CrewId == crewId && x.IsActive && !x.IsDeleted);
    }
}