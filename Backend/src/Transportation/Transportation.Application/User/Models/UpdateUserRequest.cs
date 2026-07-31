namespace Transportation.Application.User.Models;

public class UpdateUserRequest
{
    public long Id { get; init; }
    public string? Email { get; init; }
    public string UserName { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string PhoneNumber { get; init; }
    public string? TelegramId { get; init; }
    public List<string> Roles { get; init; } = [];
}