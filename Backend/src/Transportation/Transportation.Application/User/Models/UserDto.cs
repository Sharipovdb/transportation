namespace Transportation.Application.User.Models;

public record UserDto(
    long Id,
    string? Email,
    string Username,
    string Firstname,
    string LastName,
    string PhoneNumber,
    string TelegramId)
{
    public List<string> Roles { get; init; } = new();
}