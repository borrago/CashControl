using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.DeleteUser;

public class DeleteUserCommandInput(string userId) : CommandInput<DeleteUserCommandResult>
{
    public string UserId { get; } = userId;
}
