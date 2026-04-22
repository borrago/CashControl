using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(IIdentityService identityService) : ICommandHandler<ForgotPasswordCommandInput, ForgotPasswordCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<ForgotPasswordCommandResult> Handle(ForgotPasswordCommandInput request, CancellationToken cancellationToken)
    {
        var token = await _identityService.ForgotPasswordAsync(request.Email, cancellationToken);

        return (ForgotPasswordCommandResult)new ForgotPasswordCommandResult(token).WithHttpStatusCode(HttpStatusCode.OK);
    }
}
