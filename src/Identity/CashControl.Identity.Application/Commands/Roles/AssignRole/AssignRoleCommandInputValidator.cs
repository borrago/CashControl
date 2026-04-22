using FluentValidation;

namespace CashControl.Identity.Application.Commands.Roles.AssignRole;

public class AssignRoleCommandInputValidator : AbstractValidator<AssignRoleCommandInput>
{
    public AssignRoleCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().MaximumLength(100);
    }
}
