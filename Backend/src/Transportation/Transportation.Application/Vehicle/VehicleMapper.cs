using Riok.Mapperly.Abstractions;
using Transportation.Application.Vehicle.Models;

namespace Transportation.Application.Vehicle;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class VehicleMapper
{
    public partial VehicleDto Map(Domain.Entities.Vehicle entity);

    public partial List<VehicleDto> Map(List<Domain.Entities.Vehicle> entities);
}
