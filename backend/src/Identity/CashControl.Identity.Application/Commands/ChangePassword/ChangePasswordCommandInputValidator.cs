using FluentValidation;

namespace CashControl.Identity.Application.Commands.ChangePassword;

public class ChangePasswordCommandInputValidator : AbstractValidator<ChangePasswordCommandInput>
{
    public ChangePasswordCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}
