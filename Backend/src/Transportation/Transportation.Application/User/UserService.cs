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
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ApiResponse<UserDto>.Failure("The User not found", 404);

        var userDto = await MapWithRolesAsync(user);

        return ApiResponse<UserDto>.Success(userDto);
    }

    public async Task<ApiResponse<UserDto>> GetByNameAsync(string name)
    {
        var user = await userManager.FindByNameAsync(name);
        if (user is null)
            return ApiResponse<UserDto>.Failure("The User not found", 404);

        var userDto = await MapWithRolesAsync(user);

        return ApiResponse<UserDto>.Success(userDto);
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllAsync()
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync();

        // Roles are fetched one user at a time (not Task.WhenAll): GetRolesAsync hits
        // the same scoped DbContext, and running them concurrently throws
        // "A second operation was started on this context instance".
        var userDtos = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            userDtos.Add(await MapWithRolesAsync(user));
        }

        return ApiResponse<List<UserDto>>.Success(userDtos);
    }

    private async Task<UserDto> MapWithRolesAsync(Domain.Entities.User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        return userMapper.Map(user) with { Roles = roles.ToList() };
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

        var response = await userManager.CreateAsync(user, request.Password);
        if (!response.Succeeded)
        {
            return ApiResponse<string>.Failure(response.Errors.First().Description, 400);
        }

        return ApiResponse<string>.Success("User was successfully created!", 201);
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(UserDto request)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString());
        if (user is null) return ApiResponse<UserDto>.Failure("The user was not found!", 404);

        user.FirstName = request.Firstname;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Username;
        user.PhoneNumber = request.PhoneNumber;
        user.TelegramId = request.TelegramId;
        
        await userManager.UpdateAsync(user);
        return ApiResponse<UserDto>.Success(await MapWithRolesAsync(user));
    }

    public async Task<ApiResponse<string>> DeleteAsync(long userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApiResponse<string>.Failure("The user was not found!", 404);

        await userManager.DeleteAsync(user);

        return ApiResponse<string>.Success("The user was successfully deleted!", 204);
    }
}