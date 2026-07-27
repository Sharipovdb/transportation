using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class CrewMembershipByCrewIdAndUserIdSpec : Specification<Domain.Entities.CrewMembership>
{

    public CrewMembershipByCrewIdAndUserIdSpec(long crewId, long userId, bool asNoTracking = false)
    {
        
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.CrewId == crewId && x.UserId == userId && !x.IsDeleted && x.IsActive);
    }
}