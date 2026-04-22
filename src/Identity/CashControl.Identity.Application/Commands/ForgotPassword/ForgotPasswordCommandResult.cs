using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.ForgotPassword;

public class ForgotPasswordCommandResult(string token) : CommandResult
{
    public string Token { get; } = token;
}
