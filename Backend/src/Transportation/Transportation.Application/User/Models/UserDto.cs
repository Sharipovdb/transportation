namespace Transportation.Application.User.Models;

public class UserDto
{
    public long Id { get; init; }
    public string? Email { get; init; }
    public string UserName { get; init; }
    public string Fullname { get; init; }
    public string PhoneNumber { get; init; }

    public string? TelegramId { get; init; }
    public List<string> Roles { get; set; } = new List<string>();
}