using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IIdentityService identityService) : ICommandHandler<RefreshTokenCommandInput, RefreshTokenCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<RefreshTokenCommandResult> Handle(RefreshTokenCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.RefreshSessionAsync(request.UserId, cancellationToken);
        return (RefreshTokenCommandResult)new RefreshTokenCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
