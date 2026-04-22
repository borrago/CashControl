using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.Register;

public class RegisterCommandInput(string email, string password, string? fullName) : CommandInput<RegisterCommandResult>
{
    public string Email { get; } = email;
    public string Password { get; } = password;
    public string? FullName { get; } = fullName;
}
