using FluentValidation;

namespace CashControl.Identity.Application.Commands.Login;

public class LoginCommandInputValidator : AbstractValidator<LoginCommandInput>
{
    public LoginCommandInputValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
