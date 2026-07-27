using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class CrewMembershipByUserIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public long Id { get; set; }
    
    public CrewMembershipByUserIdSpec(long userId, bool asNoTracking = false)
    {
        Id = userId;
        
        if (asNoTracking)
            Query.AsNoTracking();
        
        Query.Where(c => c.UserId == userId && !c.IsDeleted);
    }
}