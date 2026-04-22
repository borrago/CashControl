using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler(IIdentityService identityService) : ICommandHandler<ConfirmEmailCommandInput, ConfirmEmailCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<ConfirmEmailCommandResult> Handle(ConfirmEmailCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);

        return (ConfirmEmailCommandResult)new ConfirmEmailCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
