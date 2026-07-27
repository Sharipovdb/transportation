using Riok.Mapperly.Abstractions;
using Transportation.Application.TransportSettings.Models;

namespace Transportation.Application.TransportSettings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class TransportSettingsMapper
{
    public partial TransportSettingsDto Map(Domain.Entities.TransportSettings transportDay);
    
    public partial List<TransportSettingsDto> Map(List<Domain.Entities.TransportSettings> transportDay);
}