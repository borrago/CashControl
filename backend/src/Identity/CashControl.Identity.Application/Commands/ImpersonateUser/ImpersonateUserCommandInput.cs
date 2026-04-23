using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.ImpersonateUser;

public class ImpersonateUserCommandInput(string userId) : CommandInput<ImpersonateUserCommandResult>
{
    public string UserId { get; } = userId;
}
