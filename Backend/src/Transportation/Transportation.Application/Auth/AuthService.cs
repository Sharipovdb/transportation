using Microsoft.AspNetCore.Identity;
using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Dtos;
using Transportation.Application.Common.Interfaces;

namespace Transportation.Application.Auth;

public class AuthService(
    UserManager<Domain.Entities.User> userManager,
    JwtTokenService jwtTokenService
) : IAuthService
{
    public async Task<ApiResponse<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.UserName);

        if (user is null)
            return ApiResponse<TokenResponse>.Failure("Wrong username or password!");

        var result = await userManager.CheckPasswordAsync(user, request.Password);

        if (!result)
            return ApiResponse<TokenResponse>.Failure("Wrong username or password!");
        
        var accessToken = await jwtTokenService.GenerateAccessToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = jwtTokenService.ComputeSha256Hash(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return ApiResponse<TokenResponse>.Success(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        }, "Logged in successfully");
    }
    
    public async Task<ApiResponse<TokenResponse>> RefreshAsync(RefreshTokenRequest request)
    {
        var hash = jwtTokenService.ComputeSha256Hash(request.RefreshToken);
        
        var user = userManager.Users
            .FirstOrDefault(x => x.RefreshToken == hash);

        if (user is null)
            return ApiResponse<TokenResponse>.Failure("Invalid refresh token");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return ApiResponse<TokenResponse>.Failure("Refresh token expired");
        
        var accessToken = await jwtTokenService.GenerateAccessToken(user);

        var newRefreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = jwtTokenService.ComputeSha256Hash(newRefreshToken);

        user.RefreshToken = refreshTokenHash;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return ApiResponse<TokenResponse>.Success(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken
        });
    }
}