using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.ConfirmEmail;

public class ConfirmEmailCommandInput(string userId, string token) : CommandInput<ConfirmEmailCommandResult>
{
    public string UserId { get; } = userId;
    public string Token { get; } = token;
}
