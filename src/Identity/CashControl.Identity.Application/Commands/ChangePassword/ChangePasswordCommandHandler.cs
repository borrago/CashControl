using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.ChangePassword;

public class ChangePasswordCommandHandler(IIdentityService identityService) : ICommandHandler<ChangePasswordCommandInput, ChangePasswordCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<ChangePasswordCommandResult> Handle(ChangePasswordCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword, cancellationToken);

        return (ChangePasswordCommandResult)new ChangePasswordCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
