using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.RefreshToken;

public class RefreshTokenCommandInput(string userId) : CommandInput<RefreshTokenCommandResult>
{
    public string UserId { get; } = userId;
}
