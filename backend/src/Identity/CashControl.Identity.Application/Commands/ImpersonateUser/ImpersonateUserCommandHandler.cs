using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.ImpersonateUser;

public class ImpersonateUserCommandHandler(IIdentityService identityService) : ICommandHandler<ImpersonateUserCommandInput, ImpersonateUserCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<ImpersonateUserCommandResult> Handle(ImpersonateUserCommandInput request, CancellationToken cancellationToken)
    {
        var auth = await _identityService.ImpersonateAsync(request.UserId, cancellationToken);

        return (ImpersonateUserCommandResult)new ImpersonateUserCommandResult(auth.AccessToken, auth.RefreshToken, auth.RefreshTokenExpiresAtUtc)
            .WithHttpStatusCode(HttpStatusCode.OK);
    }
}
