using Microsoft.AspNetCore.Http;

namespace CashControl.Core.CrossCutting;

public class AntiforgeryOptionsSettings
{
    public string HeaderName { get; set; } = "X-CSRF-TOKEN";
    public string CookieName { get; set; } = "__Host-cashcontrol-csrf";
    public string? CookieDomain { get; set; }
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Strict;
    public CookieSecurePolicy SecurePolicy { get; set; } = CookieSecurePolicy.Always;
}
