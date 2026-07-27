using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.Role.Models;
using Transportation.Shared.Authorization;

namespace Transportation.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(Roles = RoleNames.Admin)]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ApiResponse<List<string>>> GetAll()
    {
        return await _roleService.GetAllAsync();
    }

    [HttpPost("assign")]
    public async Task<ApiResponse<object>> Assign([FromBody] AssignRoleRequest request)
    {
        return await _roleService.AssignAsync(request.RoleName, request.UserId);
    }
    
    [HttpPost("remove")]
    public async Task<ApiResponse<object>> Remove([FromBody] AssignRoleRequest request)
    {
        return await _roleService.RemoveAsync(request.RoleName, request.UserId);
    }
}