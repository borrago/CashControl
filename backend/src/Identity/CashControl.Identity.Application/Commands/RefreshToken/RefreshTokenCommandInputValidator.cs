using FluentValidation;

namespace CashControl.Identity.Application.Commands.RefreshToken;

public class RefreshTokenCommandInputValidator : AbstractValidator<RefreshTokenCommandInput>
{
    public RefreshTokenCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
