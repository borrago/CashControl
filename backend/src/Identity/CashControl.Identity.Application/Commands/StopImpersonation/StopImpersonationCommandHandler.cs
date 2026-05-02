using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.StopImpersonation;

public class StopImpersonationCommandHandler(IIdentityService identityService) : ICommandHandler<StopImpersonationCommandInput, StopImpersonationCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<StopImpersonationCommandResult> Handle(StopImpersonationCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.StopImpersonationAsync(cancellationToken);
        return (StopImpersonationCommandResult)new StopImpersonationCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
