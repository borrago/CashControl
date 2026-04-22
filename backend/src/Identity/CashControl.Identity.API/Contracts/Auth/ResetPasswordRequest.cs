namespace CashControl.Identity.API.Contracts.Auth;

public record ResetPasswordRequest(string Email, string Token, string NewPassword);
