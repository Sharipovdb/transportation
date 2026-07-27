namespace Transportation.Application.User.Models;

public record UserDto(
    long Id,
    string? Email, 
    string UserName, 
    string FirstName, 
    string LastName, 
    string PhoneNumber, 
    string? TelegramId);