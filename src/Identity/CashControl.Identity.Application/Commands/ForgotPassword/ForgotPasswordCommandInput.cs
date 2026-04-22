using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.ForgotPassword;

public class ForgotPasswordCommandInput(string email) : CommandInput<ForgotPasswordCommandResult>
{
    public string Email { get; } = email;
}
