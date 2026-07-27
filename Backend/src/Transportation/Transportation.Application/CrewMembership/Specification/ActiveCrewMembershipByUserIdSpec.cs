using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class ActiveCrewMembershipByUserIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public ActiveCrewMembershipByUserIdSpec(long crewId, List<long> userIds, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
            x.CrewId == crewId &&
            userIds.Contains(x.UserId) &&
            x.IsActive && !x.IsDeleted);
    }
}