using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.Roles.AssignRole;

public class AssignRoleCommandHandler(IIdentityService identityService) : ICommandHandler<AssignRoleCommandInput, AssignRoleCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<AssignRoleCommandResult> Handle(AssignRoleCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.AssignRoleAsync(request.UserId, request.Role, cancellationToken);

        return (AssignRoleCommandResult)new AssignRoleCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
