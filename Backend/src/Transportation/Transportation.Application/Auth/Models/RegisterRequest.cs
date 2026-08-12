namespace Transportation.Application.Auth.Models;

public record RegisterRequest(
    string? Email,
    string Username,
    string Fullname,
    string Password,
    string PhoneNumber,
    string? TelegramId,
    List<string> Roles);