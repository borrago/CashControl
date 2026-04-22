namespace CashControl.Identity.API.Contracts.Api;

public sealed class AuthTokenResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
}
