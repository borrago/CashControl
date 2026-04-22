namespace CashControl.Identity.Infra.Options;

public class EmailOptions
{
    public string SenderName { get; set; } = "CashControl";
    public string SenderEmail { get; set; } = "no-reply@cashcontrol.local";
    public string? ConfirmationUrl { get; set; }
    public string? ResetPasswordUrl { get; set; }
}
