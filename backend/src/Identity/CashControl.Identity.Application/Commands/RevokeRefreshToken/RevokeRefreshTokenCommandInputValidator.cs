using FluentValidation;

namespace CashControl.Identity.Application.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandInputValidator : AbstractValidator<RevokeRefreshTokenCommandInput>
{
    public RevokeRefreshTokenCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
