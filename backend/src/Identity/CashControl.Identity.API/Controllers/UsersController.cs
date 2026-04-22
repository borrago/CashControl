using CashControl.Core.API;
using CashControl.Identity.API.Contracts.Api;
using CashControl.Identity.API.Contracts.Users;
using CashControl.Identity.Application.Commands.ChangePassword;
using CashControl.Identity.Application.Commands.RevokeRefreshToken;
using CashControl.Identity.Application.Commands.UpdateProfile;
using CashControl.Identity.Application.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashControl.Identity.API.Controllers;

[Authorize]
[ApiController]
[Route("v1/users")]
[Produces("application/json")]
public class UsersController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQueryInput(GetRequiredUserId()), cancellationToken);
        return HandleMediatorResult(result);
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateProfileCommandInput(GetRequiredUserId(), request.FullName, request.PhoneNumber),
            cancellationToken);

        return HandleMediatorResult(result);
    }

    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ChangePasswordCommandInput(GetRequiredUserId(), request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return HandleMediatorResult(result);
    }

    [HttpDelete("me/refresh-token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeRefreshToken(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RevokeRefreshTokenCommandInput(GetRequiredUserId()), cancellationToken);
        return HandleMediatorResult(result);
    }

    private string GetRequiredUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Usuario autenticado sem identificador.");
}
