using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;
using Transportation.Application.User.Models;

namespace Transportation.Application.User;

public class UserService (
    UserManager<Domain.Entities.User> userManager,
    UserMapper userMapper
    ) : IUserService
{
    public async Task<ApiResponse<UserDto>> GetByIdAsync(long userId)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);;
        if (user is null) 
            return ApiResponse<UserDto>.Failure("The User not found", 404);
        
        var userDto = userMapper.Map(user);
        
        return ApiResponse<UserDto>.Success(userDto);
    }

    public async Task<ApiResponse<UserDto>> GetByNameAsync(string name)
    {
        var user = await userManager.FindByNameAsync(name);
        if (user is null) 
            return ApiResponse<UserDto>.Failure("The User not found", 404);
        
        var userDto = userMapper.Map(user);
        
        return ApiResponse<UserDto>.Success(userDto);
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllAsync()
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync();
        var userDtos = userMapper.Map(users);

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

        var addRoleResult = await userManager.AddToRoleAsync(user, "Worker");

        if (!addRoleResult.Succeeded)
        {
            return ApiResponse<string>.Failure(
                string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
        }

        return ApiResponse<string>.Success("User created successfully", 200);
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(UserDto request)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString());
        if (user is null) return ApiResponse<UserDto>.Failure("The user was not found!", 404);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.UserName;
        user.PhoneNumber = request.PhoneNumber;
        user.TelegramId = request.TelegramId;
        
        await userManager.UpdateAsync(user);
        return ApiResponse<UserDto>.Success(userMapper.Map(user));
    }

    public async Task<ApiResponse<string>> DeleteAsync(long userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApiResponse<string>.Failure("The user was not found!", 404);

        await userManager.DeleteAsync(user);

        return ApiResponse<string>.Success("The user was successfully deleted!", 204);
    }
}