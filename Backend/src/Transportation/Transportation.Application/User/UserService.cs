using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.User.Models;

namespace Transportation.Application.User;

public class UserService (
    UserManager<Domain.Entities.User> userManager,
    UserMapper userMapper, 
    RoleManager<IdentityRole<long>> roleManager) : IUserService
{
    public async Task<ApiResponse<UserDto>> GetByIdAsync(long userId)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) 
            return ApiResponse<UserDto>.Failure("The User not found", 404);
        
        var userDto = userMapper.Map(user);
        var roles = await userManager.GetRolesAsync(user);
        userDto.Roles.AddRange(roles);
        
        return ApiResponse<UserDto>.Success(userDto);
    }
    
    public async Task<ApiResponse<List<UserDto>>> GetUsersByRoleAsync(string roleName)
    {
        var users = await userManager.GetUsersInRoleAsync(roleName);
        List<UserDto> userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var userDto = userMapper.Map(user);
            userDto.Roles.AddRange(await userManager.GetRolesAsync(user));
            userDtos.Add(userDto);
        }
        
        return ApiResponse<List<UserDto>>.Success(userDtos);
    }

    public async Task<ApiResponse<UserDto>> GetByNameAsync(string name)
    {
        var user = await userManager.FindByNameAsync(name);
        if (user is null) 
            return ApiResponse<UserDto>.Failure("The User not found", 404);
        
        var userDto = userMapper.Map(user);
        var roles = await userManager.GetRolesAsync(user);
        userDto.Roles.AddRange(roles);
        
        return ApiResponse<UserDto>.Success(userDto);
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllAsync()
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync();
        var userDtos = userMapper.Map(users);

        foreach (var userDto in userDtos)
        {
            var user = users.First(u => u.Id == userDto.Id);
            
            var roles = await userManager.GetRolesAsync(user);
            userDto.Roles = roles.ToList();
        }
        
        return ApiResponse<List<UserDto>>.Success(userDtos);
    }

    public async Task<ApiResponse<string>> CreateAsync(RegisterRequest request)
    {
        var user = new Domain.Entities.User
        {
            Email = request.Email,
            UserName = request.Username,
            FirstName = request.Firstname,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            TelegramId = request.TelegramId
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<string>.Failure($"User creation failed: {errors}");
        }

        if (request.Roles.Any())
        {
            var addRoleResult = await userManager.AddToRolesAsync(user, request.Roles);
            if (!addRoleResult.Succeeded)
            {
                return ApiResponse<string>.Failure(
                    string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
            }
        }

        return ApiResponse<string>.Success("User created successfully", 200);
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(UpdateUserRequest request)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString());
        if (user is null) return ApiResponse<UserDto>.Failure("The user was not found!", 404);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.UserName;
        user.PhoneNumber = request.PhoneNumber;
        user.TelegramId = request.TelegramId;
        
        var currentRoles = await userManager.GetRolesAsync(user);
        var result = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!result.Succeeded)
        {
            return ApiResponse<UserDto>.Failure(
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    
        if (request.Roles.Any())
        {
            var addRoleResult = await userManager.AddToRolesAsync(user, request.Roles);
            if (!addRoleResult.Succeeded)
            {
                return ApiResponse<UserDto>.Failure(
                    string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
            }
        }
        
        await userManager.UpdateAsync(user);
        var userDto = userMapper.Map(user);
        userDto.Roles = request.Roles;
        
        return ApiResponse<UserDto>.Success(userDto);
    }

    public async Task<ApiResponse<string>> DeleteAsync(long userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApiResponse<string>.Failure("The user was not found!", 404);

        await userManager.DeleteAsync(user);

        return ApiResponse<string>.Success("The user was successfully deleted!", 204);
    }
}