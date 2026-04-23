using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.ImpersonateUser;

public class ImpersonateUserCommandResult(string? accessToken, string? refreshToken, DateTime? refreshTokenExpiresAtUtc) : CommandResult
{
    public string? AccessToken { get; } = accessToken;
    public string? RefreshToken { get; } = refreshToken;
    public DateTime? RefreshTokenExpiresAtUtc { get; } = refreshTokenExpiresAtUtc;
}
