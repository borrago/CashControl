using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.Roles.AssignRole;

public class AssignRoleCommandInput(string userId, string role) : CommandInput<AssignRoleCommandResult>
{
    public string UserId { get; } = userId;
    public string Role { get; } = role;
}
