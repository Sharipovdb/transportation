using MediatR;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.User.Models;
using Transportation.Application.User.Queries;

namespace Transportation.API.Controllers;

public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IMediator mediator, IUserService userService) : base(mediator)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ApiResponse<UserDto>> GetMe()
    {
        return await _mediator.Send(new GetCurrentUser());
    }

    [HttpGet]
    public async Task<ApiResponse<List<UserDto>>> GetAll()
    {
        return await _userService.GetAllAsync();
    }

    [HttpGet("{userId:long}")]
    public async Task<ApiResponse<UserDto>> GetById(long userId)
    {
        return await _userService.GetByIdAsync(userId);
    }

    [HttpGet("by-name/{username}")]
    public async Task<ApiResponse<UserDto>> GetByName(string username)
    {
        return await _userService.GetByNameAsync(username);
    }

    [HttpPost]
    public async Task<ApiResponse<string>> Create([FromBody] RegisterRequest request)
    {
        return await _userService.CreateAsync(request);
    }

    [HttpPut]
    public async Task<ApiResponse<UserDto>> Update([FromBody] UserDto request)
    {
        return await _userService.UpdateAsync(request);
    }

    [HttpDelete("{userId:long}")]
    public async Task<ApiResponse<string>> Delete(long userId)
    {
        return await _userService.DeleteAsync(userId);
    }
}