using CashControl.Identity.Application.Services;
using Microsoft.Extensions.Logging;

namespace CashControl.Identity.Infra.Services;

public class LoggingIdentityEmailSender(ILogger<LoggingIdentityEmailSender> logger) : IIdentityEmailSender
{
    private readonly ILogger<LoggingIdentityEmailSender> _logger = logger;

    public Task SendAsync(IdentityEmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Identity email queued. To: {ToEmail}. Subject: {Subject}. Body: {Body}",
            message.ToEmail,
            message.Subject,
            message.Body);

        return Task.CompletedTask;
    }
}
