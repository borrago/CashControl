using FluentValidation;

namespace CashControl.Identity.Application.Queries.GetUserById;

public class GetUserByIdQueryInputValidator : AbstractValidator<GetUserByIdQueryInput>
{
    public GetUserByIdQueryInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
