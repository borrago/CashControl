using CashControl.Core.API;
using CashControl.Identity.API.Contracts.Auth;
using CashControl.Identity.Application.Commands.ConfirmEmail;
using CashControl.Identity.Application.Commands.ForgotPassword;
using CashControl.Identity.Application.Commands.Login;
using CashControl.Identity.Application.Commands.RefreshToken;
using CashControl.Identity.Application.Commands.Register;
using CashControl.Identity.Application.Commands.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace CashControl.Identity.API.Controllers;

[ApiController]
[Route("v1/auth")]
[Produces("application/json")]
public class AuthController(IMediator mediator, IAntiforgery antiforgery) : BaseController
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly IAntiforgery _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));

    [AllowAnonymous]
    [HttpGet("csrf-token")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new
        {
            requestToken = tokens.RequestToken,
            headerName = tokens.HeaderName
        });
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
    [EnableRateLimiting("AuthSensitive")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterCommandInput(request.Email, request.Password, request.FullName),
            cancellationToken);

        return HandleMediatorResult(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
    [EnableRateLimiting("AuthSensitive")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommandInput(request.Email, request.Password),
            cancellationToken);

        return HandleMediatorResult(result);
    }

    [Authorize]
    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ValidateCsrfToken]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshTokenCommandInput(GetRequiredUserId()), cancellationToken);

        return HandleMediatorResult(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
    [EnableRateLimiting("AuthSensitive")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ForgotPasswordCommandInput(request.Email), cancellationToken);
        return HandleMediatorResult(result);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
    [EnableRateLimiting("AuthSensitive")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ResetPasswordCommandInput(request.Email, request.Token, request.NewPassword),
            cancellationToken);

        return HandleMediatorResult(result);
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmEmailCommandInput(request.UserId, request.Token), cancellationToken);
        return HandleMediatorResult(result);
    }

    private string GetRequiredUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Usuario autenticado sem identificador.");
}
