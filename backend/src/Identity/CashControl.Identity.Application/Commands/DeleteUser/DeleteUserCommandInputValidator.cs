using FluentValidation;

namespace CashControl.Identity.Application.Commands.DeleteUser;

public class DeleteUserCommandInputValidator : AbstractValidator<DeleteUserCommandInput>
{
    public DeleteUserCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
