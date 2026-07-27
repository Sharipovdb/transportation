using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class CrewMembershipByIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public long Id { get; set; }
    
    public CrewMembershipByIdSpec(long id, bool asNoTracking = false)
    {
        Id = id;
        
        if(asNoTracking)
            Query.AsNoTracking();
        
        Query.Where(c => c.Id == id && !c.IsDeleted);
    }
}