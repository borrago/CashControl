namespace CashControl.Identity.Infra.Options;

public class SecurityOptions
{
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int DefaultLockoutMinutes { get; set; } = 15;
    public int AuthRateLimitPermitLimit { get; set; } = 30;
    public int AuthRateLimitWindowSeconds { get; set; } = 60;
}
