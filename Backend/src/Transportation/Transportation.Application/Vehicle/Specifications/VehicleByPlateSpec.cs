using Ardalis.Specification;

namespace Transportation.Application.Vehicle.Specifications;

public sealed class VehicleByPlateSpec : Specification<Domain.Entities.Vehicle>
{
    public string Plate { get; set; }

    public VehicleByPlateSpec(string plate, bool asNoTracking = false)
    {
        Plate = plate;

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.Plate == plate && !x.IsDeleted);
    }
}