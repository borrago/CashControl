using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.Login;

public class LoginCommandHandler(IIdentityService identityService) : ICommandHandler<LoginCommandInput, LoginCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<LoginCommandResult> Handle(LoginCommandInput request, CancellationToken cancellationToken)
    {
        var login = await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);

        return (LoginCommandResult)new LoginCommandResult(login.AccessToken, login.RefreshToken, login.RefreshTokenExpiresAtUtc).WithHttpStatusCode(HttpStatusCode.OK);
    }
}
