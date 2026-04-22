namespace CashControl.Identity.API.Contracts.Api;

public sealed class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public IList<string> Roles { get; set; } = [];
}
