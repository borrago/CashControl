using FluentValidation;

namespace CashControl.Identity.Application.Commands.ImpersonateUser;

public class ImpersonateUserCommandInputValidator : AbstractValidator<ImpersonateUserCommandInput>
{
    public ImpersonateUserCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
