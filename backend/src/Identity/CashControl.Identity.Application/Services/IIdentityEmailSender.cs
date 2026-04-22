namespace CashControl.Identity.Application.Services;

public interface IIdentityEmailSender
{
    Task SendAsync(IdentityEmailMessage message, CancellationToken cancellationToken = default);
}
