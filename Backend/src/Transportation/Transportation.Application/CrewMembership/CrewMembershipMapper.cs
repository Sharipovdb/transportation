using Riok.Mapperly.Abstractions;
using Transportation.Application.CrewMembership.Models;

namespace Transportation.Application.CrewMembership;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class CrewMembershipMapper
{
    public partial CrewMembershipDto Map(Domain.Entities.CrewMembership entity);

    public partial List<CrewMembershipDto> Map(List<Domain.Entities.CrewMembership> entities);
}