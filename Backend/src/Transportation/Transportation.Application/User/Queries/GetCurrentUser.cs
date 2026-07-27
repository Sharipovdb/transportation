using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.User.Models;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;
using Transportation.Shared;
using Transportation.Shared.Extensions;
using Transportation.Shared.Middlewares;

namespace Transportation.Application.User.Queries;

public sealed record GetCurrentUser : IQuery<ApiResponse<UserDto>>;

internal sealed class GetCurrentUserHandler : IQueryHandler<GetCurrentUser, ApiResponse<UserDto>>
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUserService _userService;

    public GetCurrentUserHandler(ICurrentUserAccessor currentUserAccessor, IUserService userService)
    {
        _currentUserAccessor = currentUserAccessor;
        _userService = userService;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetCurrentUser request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.GetRequiredUser().GetUserId();

        var user = await _userService.GetByIdAsync(userId);

        if (user is null)
            throw new ResourceNotFoundException(UserError.NotFound);

        return user;
    }
}
