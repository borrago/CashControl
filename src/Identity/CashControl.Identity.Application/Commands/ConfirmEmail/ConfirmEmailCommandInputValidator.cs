using FluentValidation;

namespace CashControl.Identity.Application.Commands.ConfirmEmail;

public class ConfirmEmailCommandInputValidator : AbstractValidator<ConfirmEmailCommandInput>
{
    public ConfirmEmailCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}
