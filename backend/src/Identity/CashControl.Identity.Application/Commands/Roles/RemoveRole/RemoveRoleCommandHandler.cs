using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.Roles.RemoveRole;

public class RemoveRoleCommandHandler(IIdentityService identityService) : ICommandHandler<RemoveRoleCommandInput, RemoveRoleCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<RemoveRoleCommandResult> Handle(RemoveRoleCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.RemoveRoleAsync(request.UserId, request.Role, cancellationToken);

        return (RemoveRoleCommandResult)new RemoveRoleCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
