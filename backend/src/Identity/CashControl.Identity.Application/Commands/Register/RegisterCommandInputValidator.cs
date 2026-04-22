using FluentValidation;

namespace CashControl.Identity.Application.Commands.Register;

public class RegisterCommandInputValidator : AbstractValidator<RegisterCommandInput>
{
    public RegisterCommandInputValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FullName).MaximumLength(200);
    }
}
