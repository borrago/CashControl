using FluentValidation;

namespace CashControl.Identity.Application.Commands.ForgotPassword;

public class ForgotPasswordCommandInputValidator : AbstractValidator<ForgotPasswordCommandInput>
{
    public ForgotPasswordCommandInputValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
