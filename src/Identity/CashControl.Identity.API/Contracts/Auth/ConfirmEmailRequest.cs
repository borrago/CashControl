namespace CashControl.Identity.API.Contracts.Auth;

public record ConfirmEmailRequest(string UserId, string Token);
