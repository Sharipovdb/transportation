using Riok.Mapperly.Abstractions;
using Transportation.Application.User.Models;

namespace Transportation.Application.User;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class UserMapper
{
    public partial UserDto Map(Domain.Entities.User user);
    public partial List<UserDto> Map(List<Domain.Entities.User> users);
}