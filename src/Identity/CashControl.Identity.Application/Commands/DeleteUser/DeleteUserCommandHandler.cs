using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.DeleteUser;

public class DeleteUserCommandHandler(IIdentityService identityService) : ICommandHandler<DeleteUserCommandInput, DeleteUserCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<DeleteUserCommandResult> Handle(DeleteUserCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.DeleteUserAsync(request.UserId, cancellationToken);

        return (DeleteUserCommandResult)new DeleteUserCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
