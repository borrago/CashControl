namespace CashControl.Identity.API.Contracts.Auth;

public record RegisterRequest(string Email, string Password, string? FullName);
