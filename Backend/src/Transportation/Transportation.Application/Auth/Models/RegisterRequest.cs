namespace Transportation.Application.Auth.Models;

public record RegisterRequest(
    string Email, 
    string Username, 
    string Firstname, 
    string LastName, 
    string Password, 
    string PhoneNumber, 
    string TelegramId);