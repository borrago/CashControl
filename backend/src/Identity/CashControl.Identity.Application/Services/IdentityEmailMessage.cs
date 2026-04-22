namespace CashControl.Identity.Application.Services;

public sealed record IdentityEmailMessage(
    string ToEmail,
    string Subject,
    string Body);
