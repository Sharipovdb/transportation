using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public class CrewMembershipByIdAndUserId : Specification<Domain.Entities.CrewMembership>
{
    public long LongId { get; set; }
    public long UserId { get; set; }

    public CrewMembershipByIdAndUserId(long crewId, long userId, bool asNoTracking = false)
    {
        LongId = crewId;
        UserId = userId;
        
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(c => c.CrewId == crewId && c.UserId == userId);
    }
}