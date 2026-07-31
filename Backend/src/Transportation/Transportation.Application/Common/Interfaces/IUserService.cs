using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Dtos;
using Transportation.Application.User.Models;

namespace Transportation.Application.Common.Interfaces;

public interface IUserService
{
    public Task<ApiResponse<UserDto>> GetByIdAsync(long userId);
    public Task<ApiResponse<UserDto>> GetByNameAsync(string name);
    public Task<ApiResponse<List<UserDto>>> GetUsersByRoleAsync(string roleName);
    public Task<ApiResponse<List<UserDto>>> GetAllAsync();
    public Task<ApiResponse<string>> CreateAsync(RegisterRequest  request);
    public Task<ApiResponse<UserDto>> UpdateAsync(UpdateUserRequest user);
    public Task<ApiResponse<string>> DeleteAsync(long userId);
}