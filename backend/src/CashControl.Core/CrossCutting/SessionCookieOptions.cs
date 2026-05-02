using Microsoft.AspNetCore.Http;

namespace CashControl.Core.CrossCutting;

public class SessionCookieOptions
{
    public string Name { get; set; } = "__Host-cashcontrol-session";
    public string? Domain { get; set; }
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
    public CookieSecurePolicy SecurePolicy { get; set; } = CookieSecurePolicy.Always;
    public bool SlidingExpiration { get; set; } = true;
    public int ExpireHours { get; set; } = 8;
}
