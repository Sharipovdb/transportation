
using Ardalis.Specification;

namespace Transportation.Application.Vehicle.Specifications;

public sealed class VehicleByDriverIdSpec : Specification<Domain.Entities.Vehicle>
{
    public VehicleByDriverIdSpec(long driverId, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.DriverId == driverId && !x.IsDeleted);
    }
}