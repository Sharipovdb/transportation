using Microsoft.AspNetCore.Mvc;
using Transportation.Application.Auth.Models;
using Transportation.Application.Common.Interfaces;
using LoginRequest = Transportation.Application.Auth.Models.LoginRequest;
using RegisterRequest = Transportation.Application.Auth.Models.RegisterRequest;

namespace Transportation.API.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshAsync(request);

        return Ok(result);
    }
}