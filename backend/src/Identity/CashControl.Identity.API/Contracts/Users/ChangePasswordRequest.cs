namespace CashControl.Identity.API.Contracts.Users;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
