using FluentValidation;

namespace CashControl.Identity.Application.Commands.UpdateProfile;

public class UpdateProfileCommandInputValidator : AbstractValidator<UpdateProfileCommandInput>
{
    public UpdateProfileCommandInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
    }
}
