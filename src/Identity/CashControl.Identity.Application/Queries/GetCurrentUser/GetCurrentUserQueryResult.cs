using CashControl.Core.Application;

namespace CashControl.Identity.Application.Queries.GetCurrentUser;

public class GetCurrentUserQueryResult : QueryResult, IQueryResultItem
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public IList<string> Roles { get; set; } = [];
}
