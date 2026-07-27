using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class CrewMembershipByCrewIdAndUserIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public long CrewId { get; set; }

    public CrewMembershipByCrewIdAndUserIdSpec(long crewId, long? userId, bool asNoTracking = false)
    {
        CrewId = crewId;
        
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.CrewId == crewId && !x.IsDeleted && x.IsActive);

        if (userId.HasValue)
            Query.Where(x => x.UserId == userId.Value);
    }
}