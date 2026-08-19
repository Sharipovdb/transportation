using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.User.Models;
using Transportation.Application.User.Queries;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IMediator mediator, IUserService userService) : base(mediator)
    {
        _userService = userService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ApiResponse<UserDto>> GetMe()
    {
        return await _mediator.Send(new GetCurrentUser());
    }

    [HttpGet]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager, RoleNames.Accountant)]
    public async Task<ApiResponse<List<UserDto>>> GetAll()
    {
        return await _userService.GetAllAsync();
    }

    [HttpGet("{userId:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager, RoleNames.Accountant)]
    public async Task<ApiResponse<UserDto>> GetById(long userId)
    {
        return await _userService.GetByIdAsync(userId);
    }

    [HttpGet("{username}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager, RoleNames.Accountant)]
    public async Task<ApiResponse<UserDto>> GetByName(string username)
    {
        return await _userService.GetByNameAsync(username);
    }

    [HttpGet("{roleName}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager, RoleNames.Accountant)]
    public async Task<ApiResponse<List<UserDto>>> GetUsersByRole(string roleName)
    {
        return await _userService.GetUsersByRoleAsync(roleName);
    }

    [HttpPost]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<ApiResponse<string>> Create([FromBody] RegisterRequest request)
    {
        return await _userService.CreateAsync(request);
    }

    [HttpPut]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<ApiResponse<UserDto>> Update([FromBody] UpdateUserRequest request)
    {
        return await _userService.UpdateAsync(request);
    }

    [HttpDelete("{userId:long}")]
    [RoleAuthorize(RoleNames.Admin, RoleNames.RouteManager)]
    public async Task<ApiResponse<string>> Delete(long userId)
    {
        return await _userService.DeleteAsync(userId);
    }
}