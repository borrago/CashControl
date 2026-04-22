using CashControl.Core.API;
using CashControl.Identity.Application.Commands.DeleteUser;
using CashControl.Identity.Application.Commands.Roles.AssignRole;
using CashControl.Identity.Application.Commands.Roles.RemoveRole;
using CashControl.Identity.Application.Queries.GetUserById;
using CashControl.Identity.Application.Queries.GetUserRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashControl.Identity.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("v1/admin/users")]
[Produces("application/json")]
public class AdminUsersController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(string userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQueryInput(userId), cancellationToken);
        return HandleMediatorResult(result);
    }

    [HttpGet("{userId}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(string userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserRolesQueryInput(userId), cancellationToken);
        return HandleMediatorResult(result);
    }

    [HttpPut("{userId}/roles/{role}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignRole(string userId, string role, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AssignRoleCommandInput(userId, role), cancellationToken);
        return HandleMediatorResult(result);
    }

    [HttpDelete("{userId}/roles/{role}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRole(string userId, string role, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveRoleCommandInput(userId, role), cancellationToken);
        return HandleMediatorResult(result);
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(string userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteUserCommandInput(userId), cancellationToken);
        return HandleMediatorResult(result);
    }
}
