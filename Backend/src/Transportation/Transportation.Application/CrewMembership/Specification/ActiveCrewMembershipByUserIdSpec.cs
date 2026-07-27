using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class ActiveCrewMembershipByUserIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public long UserId { get; private set; }

    public ActiveCrewMembershipByUserIdSpec(long userId, bool asNoTracking = false)
    {
        UserId = userId;
        
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.UserId == this.UserId && x.IsActive && !x.IsDeleted);
    }
}