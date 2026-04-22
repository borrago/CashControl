using FluentValidation;

namespace CashControl.Identity.Application.Commands.Roles.RemoveRole;

public class RemoveRoleCommandInputValidator : AbstractValidator<RemoveRoleCommandInput>
{
    public RemoveRoleCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().MaximumLength(100);
    }
}
