using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandHandler(IIdentityService identityService) : ICommandHandler<RevokeRefreshTokenCommandInput, RevokeRefreshTokenCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<RevokeRefreshTokenCommandResult> Handle(RevokeRefreshTokenCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.SignOutAsync(request.UserId, cancellationToken);

        return (RevokeRefreshTokenCommandResult)new RevokeRefreshTokenCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
