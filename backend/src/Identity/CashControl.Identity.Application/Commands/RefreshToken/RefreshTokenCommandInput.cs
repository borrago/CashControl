using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.RefreshToken;

public class RefreshTokenCommandInput(string accessToken, string refreshToken) : CommandInput<RefreshTokenCommandResult>
{
    public string AccessToken { get; } = accessToken;
    public string RefreshToken { get; } = refreshToken;
}
