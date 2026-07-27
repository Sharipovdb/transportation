using Ardalis.Specification;

namespace Transportation.Application.Crew.Specification;

public sealed class CrewByIdSpec : Specification<Transportation.Domain.Entities.Crew>
{
    public long Id { get; set; }
    
    public CrewByIdSpec(long id, bool asNoTracking = false)
    {
        Id = id;
        
        if (asNoTracking)
            Query.AsNoTracking();
        
        Query.Where(x => x.Id == id && !x.IsDeleted);
        
        Query.Include(x => x.DriverLead);
        Query.Include(x => x.Route);
    }
}