using Transportation.Application.Common.Dtos;

namespace Transportation.Application.Common.Interfaces;

public interface IRoleService
{
    public Task<ApiResponse<string>> CreateAsync(string roleName);
    public Task<ApiResponse<object>> DeleteAsync(string roleName);
    public Task<ApiResponse<object>> AssignAsync(string roleName, long userId);
    public Task<ApiResponse<object>> RemoveAsync(string roleName, long userId);
    public Task<ApiResponse<List<string>>> GetAllAsync();
}