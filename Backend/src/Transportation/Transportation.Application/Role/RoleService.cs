using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;

namespace Transportation.Application.Role;

public class RoleService (
    RoleManager<IdentityRole<long>> roleManager,
    UserManager<Domain.Entities.User> userManager
    ) : IRoleService
{
    public async Task<ApiResponse<string>> CreateAsync(string roleName)
    {
        var response = await roleManager.FindByNameAsync(roleName);
        if (response is not null) return ApiResponse<string>.Failure("The Role is already created!");
        
        await roleManager.CreateAsync(new IdentityRole<long>(roleName));
        return ApiResponse<string>.Success("The Role created!", 201);
    }

    public async Task<ApiResponse<object>> DeleteAsync(string roleName)
    {
        var response = await roleManager.FindByNameAsync(roleName);
        if (response is null) return ApiResponse<object>.Failure("The Role doesn't exist!");
        
        await roleManager.DeleteAsync(response);
        
        return ApiResponse<object>.Success("The Role deleted!", 204);
    }

    public async Task<ApiResponse<object>> AssignAsync(string roleName, long userId)
    {
        var role = await roleManager.RoleExistsAsync(roleName);
        if (!role) return ApiResponse<object>.Failure("The Role not found!", 404);
        
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApiResponse<object>.Failure("The User not found!", 404);
        
        await userManager.AddToRoleAsync(user, roleName);
        return ApiResponse<object>.Success($"The User was successfully assigned into role {roleName}", 200);
    }

    public async Task<ApiResponse<object>> RemoveAsync(string roleName, long userId)
    {
        var role = await roleManager.RoleExistsAsync(roleName);
        if (!role) return ApiResponse<object>.Failure("The Role not found!", 404);
        
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApiResponse<object>.Failure("The User not found!", 404);
        
        var result = await userManager.IsInRoleAsync(user, roleName);
        if (!result) return ApiResponse<object>.Failure("The User is not in this Role!", 400);
        
        await userManager.RemoveFromRoleAsync(user, roleName);
        return ApiResponse<object>.Success("The User was successfully unassigned from this Role!", 200); 
    }

    public async Task<ApiResponse<List<string>>> GetAllAsync()
    {
        var roles = await roleManager.Roles.AsNoTracking()
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .ToListAsync();
        
        return ApiResponse<List<string>>.Success(roles);
    }
}