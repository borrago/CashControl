using CashControl.Core.Application;

namespace CashControl.Identity.Application.Queries.GetCurrentUser;

public class GetCurrentUserQueryInput(string Id) : QueryInput<GetCurrentUserQueryResult>
{
    public string Id { get; } = Id;
}
