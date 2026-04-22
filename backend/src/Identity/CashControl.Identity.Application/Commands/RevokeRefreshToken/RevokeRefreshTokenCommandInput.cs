using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandInput(string userId) : CommandInput<RevokeRefreshTokenCommandResult>
{
    public string UserId { get; } = userId;
}
