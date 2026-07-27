using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class CrewMembershipByIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public CrewMembershipByIdSpec(long id, bool asNoTracking = false)
    {
        if(asNoTracking)
            Query.AsNoTracking();
        
        Query.Where(c => c.Id == id && !c.IsDeleted);
    }
}