using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.Login;

public class LoginCommandInput(string email, string password) : CommandInput<LoginCommandResult>
{
    public string Email { get; } = email;
    public string Password { get; } = password;
}
