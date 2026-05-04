using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using CoreApplicationException = CashControl.Core.Application.ApplicationException;
using System.Net;

namespace CashControl.Identity.Application.Commands.Login;

public class LoginCommandHandler(IIdentityService identityService) : ICommandHandler<LoginCommandInput, LoginCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<LoginCommandResult> Handle(LoginCommandInput request, CancellationToken cancellationToken)
    {
        try
        {
            await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);
            return (LoginCommandResult)new LoginCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
        }
        catch (CoreApplicationException ex)
        {
            return (LoginCommandResult)new LoginCommandResult()
                .WithErrors(ex.Errors)
                .WithHttpStatusCode(HttpStatusCode.BadRequest);
        }
    }
}
