using CashControl.Identity.Application.Services;

namespace CashControl.Identity.API.IntegrationTests;

public class TestIdentityEmailSender : IIdentityEmailSender
{
    private readonly List<IdentityEmailMessage> _messages = [];
    private readonly object _sync = new();

    public Task SendAsync(IdentityEmailMessage message, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            _messages.Add(message);
        }

        return Task.CompletedTask;
    }

    public IdentityEmailMessage? GetLastMessage(string email, string subject)
    {
        lock (_sync)
        {
            return _messages.LastOrDefault(message =>
                string.Equals(message.ToEmail, email, StringComparison.OrdinalIgnoreCase) &&
                message.Subject.Contains(subject, StringComparison.OrdinalIgnoreCase));
        }
    }
}
