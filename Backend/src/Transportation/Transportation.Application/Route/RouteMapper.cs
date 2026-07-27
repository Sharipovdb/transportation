using Riok.Mapperly.Abstractions;
using Transportation.Application.Route.Models;

namespace Transportation.Application.Route;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class RouteMapper
{
    public partial RouteDto Map(Domain.Entities.Route entity);
    
    public partial List<RouteDto> Map(List<Domain.Entities.Route> entities);
}