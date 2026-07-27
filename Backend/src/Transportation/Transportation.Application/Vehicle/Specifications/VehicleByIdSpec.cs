using Ardalis.Specification;

namespace Transportation.Application.Vehicle.Specifications;

public sealed class VehicleByIdSpec : Specification<Domain.Entities.Vehicle>
{
    public long Id { get; set; }
    
    public VehicleByIdSpec(long id, bool asNoTracking = false)
    {
        Id = id;
        
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.Id == id && !x.IsDeleted);
    }
}