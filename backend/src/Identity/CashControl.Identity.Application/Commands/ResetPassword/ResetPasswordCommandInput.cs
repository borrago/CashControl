using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.ResetPassword;

public class ResetPasswordCommandInput(string email, string token, string newPassword) : CommandInput<ResetPasswordCommandResult>
{
    public string Email { get; } = email;
    public string Token { get; } = token;
    public string NewPassword { get; } = newPassword;
}
