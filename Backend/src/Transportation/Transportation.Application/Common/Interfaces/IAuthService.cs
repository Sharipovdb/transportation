using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Dtos;

namespace Transportation.Application.Common.Interfaces;

public interface IAuthService
{
    public Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);

    Task<ApiResponse<TokenResponse>> LoginAsync(LoginRequest request);

    Task<ApiResponse<TokenResponse>> RefreshAsync(RefreshTokenRequest request);
}