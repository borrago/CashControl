using CashControl.Core.Application;

namespace CashControl.Identity.Application.Queries.GetUserRoles;

public class GetUserRolesQueryResult : QueryResult, IQueryResultItem
{
    public string UserId { get; set; } = default!;
    public IList<string> Roles { get; set; } = [];
}
