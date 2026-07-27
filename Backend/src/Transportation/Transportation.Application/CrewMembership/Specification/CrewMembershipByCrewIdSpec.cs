using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public class CrewMembershipByCrewIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public CrewMembershipByCrewIdSpec(long crewId, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.CrewId == crewId && !x.IsDeleted);
    }
}