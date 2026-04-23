using CashControl.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace CashControl.Identity.Domain.Entities;

public class User : IdentityUser, IAggregateRoot, ITenantEntity
{
    public const int DefaultTenant = 1;

    public string? FullName { get; set; }
    public int Tenant { get; set; } = DefaultTenant;
    public bool IsSuperUser { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTimeUtc { get; set; }

    public void UpdateProfile(string? fullName, string? phoneNumber)
    {
        FullName = fullName?.Trim();
        PhoneNumber = phoneNumber?.Trim();
    }

    public void SetRefreshToken(string refreshToken, DateTime refreshTokenExpiryTimeUtc)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTimeUtc = refreshTokenExpiryTimeUtc;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTimeUtc = null;
    }
}
