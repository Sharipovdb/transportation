using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Transportation.Application.Auth;

public class JwtTokenService(
    UserManager<Domain.Entities.User> userManager,
    IOptions<JwtOptions> options
)
{
    private readonly JwtOptions _jwt = options.Value;

    public async Task<string> GenerateAccessToken(Domain.Entities.User user)
    {
        var userRoles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber ?? ""),
            new Claim("telegramId", user.TelegramId ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, userRoles.FirstOrDefault() ?? ""),
        };

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public  string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];

        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
    
    public string ComputeSha256Hash(string value)
    {
        using var sha = SHA256.Create();
    
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
    
        return Convert.ToHexString(bytes);
    }
}