using CashControl.Core.Application;

namespace CashControl.Identity.Application.Queries.GetUserById;

public class GetUserByIdQueryResult : QueryResult, IQueryResultItem
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public int Tenant { get; set; }
    public bool IsSuperUser { get; set; }
    public IList<string> Roles { get; set; } = [];
}
