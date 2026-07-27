using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Dtos;

namespace Transportation.Application.Common.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<TokenResponse>> LoginAsync(LoginRequest request);

    Task<ApiResponse<TokenResponse>> RefreshAsync(RefreshTokenRequest request);
}