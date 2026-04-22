using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.Roles.RemoveRole;

public class RemoveRoleCommandInput(string userId, string role) : CommandInput<RemoveRoleCommandResult>
{
    public string UserId { get; } = userId;
    public string Role { get; } = role;
}
