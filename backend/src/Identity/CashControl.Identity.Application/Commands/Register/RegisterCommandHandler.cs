using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.Register;

public class RegisterCommandHandler(IIdentityService identityService) : ICommandHandler<RegisterCommandInput, RegisterCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<RegisterCommandResult> Handle(RegisterCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.RegisterAsync(request.Email, request.Password, request.FullName, cancellationToken);

        return (RegisterCommandResult)new RegisterCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
