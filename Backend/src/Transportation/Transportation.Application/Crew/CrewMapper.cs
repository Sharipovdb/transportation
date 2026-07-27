using Transportation.Application.Crew.Models;
using Riok.Mapperly.Abstractions;

namespace Transportation.Application.Crew;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class CrewMapper
{
    public partial CrewDto Map(Domain.Entities.Crew crew);
    public partial List<CrewDto> Map(List<Domain.Entities.Crew> crews);
}