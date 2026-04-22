using FluentValidation;

namespace CashControl.Identity.Application.Commands.ResetPassword;

public class ResetPasswordCommandInputValidator : AbstractValidator<ResetPasswordCommandInput>
{
    public ResetPasswordCommandInputValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}
