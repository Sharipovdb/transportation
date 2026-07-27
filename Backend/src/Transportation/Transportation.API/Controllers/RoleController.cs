using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.Role.Models;

namespace Transportation.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

    [HttpPost]
    public async Task<ApiResponse<string>> Create([FromBody] RoleRequest request)
    {
        return await _roleService.CreateAsync(request.RoleName);
    }

    [HttpDelete("{roleName}")]
    public async Task<ApiResponse<object>> Delete(string roleName)
    {
        return await _roleService.DeleteAsync(roleName);
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