using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.ResetPassword;

public class ResetPasswordCommandHandler(IIdentityService identityService) : ICommandHandler<ResetPasswordCommandInput, ResetPasswordCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<ResetPasswordCommandResult> Handle(ResetPasswordCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);

        return (ResetPasswordCommandResult)new ResetPasswordCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
