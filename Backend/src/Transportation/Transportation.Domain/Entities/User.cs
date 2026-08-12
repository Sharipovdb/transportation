using Microsoft.AspNetCore.Identity;

namespace Transportation.Domain.Entities;

public class User : IdentityUser<long>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    // Not a column — the API exposes one name field, not two, so this is what gets
    // mapped onto it. Storage stays split because Identity seeding and a few tests
    // build FirstName/LastName directly.
    public string Fullname => $"{FirstName} {LastName}".Trim();

    public string? TelegramId { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}