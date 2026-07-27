using Microsoft.AspNetCore.Identity;

namespace Transportation.Domain.Entities;

public class User : IdentityUser<long>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string TelegramId { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}