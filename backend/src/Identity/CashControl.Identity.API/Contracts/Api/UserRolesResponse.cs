namespace CashControl.Identity.API.Contracts.Api;

public sealed class UserRolesResponse
{
    public string UserId { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = [];
}
