using Ardalis.Specification;

namespace Transportation.Application.Crew.Specification;

public sealed class CrewByIdSpec : Specification<Transportation.Domain.Entities.Crew>
{
    
    public CrewByIdSpec(long id, bool asNoTracking = false)
    {
        
        if (asNoTracking)
            Query.AsNoTracking();
        
        Query.Where(x => x.Id == id && !x.IsDeleted);
        
        Query.Include(x => x.DriverLead);
        Query.Include(x => x.Route);
    }
}