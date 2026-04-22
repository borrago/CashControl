using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(IIdentityService identityService) : ICommandHandler<UpdateProfileCommandInput, UpdateProfileCommandResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<UpdateProfileCommandResult> Handle(UpdateProfileCommandInput request, CancellationToken cancellationToken)
    {
        await _identityService.UpdateProfileAsync(request.UserId, request.FullName, request.PhoneNumber, cancellationToken);

        return (UpdateProfileCommandResult)new UpdateProfileCommandResult().WithHttpStatusCode(HttpStatusCode.NoContent);
    }
}
