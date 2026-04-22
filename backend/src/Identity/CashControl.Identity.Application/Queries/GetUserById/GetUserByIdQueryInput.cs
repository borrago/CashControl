using CashControl.Core.Application;

namespace CashControl.Identity.Application.Queries.GetUserById;

public class GetUserByIdQueryInput(string userId) : QueryInput<GetUserByIdQueryResult>
{
    public string UserId { get; } = userId;
}
