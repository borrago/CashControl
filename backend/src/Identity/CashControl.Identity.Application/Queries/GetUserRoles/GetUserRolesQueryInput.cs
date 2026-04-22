using CashControl.Core.Application;

namespace CashControl.Identity.Application.Queries.GetUserRoles;

public class GetUserRolesQueryInput(string userId) : QueryInput<GetUserRolesQueryResult>
{
    public string UserId { get; } = userId;
}
