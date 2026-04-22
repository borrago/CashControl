using FluentValidation;

namespace CashControl.Identity.Application.Queries.GetUserRoles;

public class GetUserRolesQueryInputValidator : AbstractValidator<GetUserRolesQueryInput>
{
    public GetUserRolesQueryInputValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
