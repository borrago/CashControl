using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.UpdateProfile;

public class UpdateProfileCommandInput(string userId, string? fullName, string? phoneNumber) : CommandInput<UpdateProfileCommandResult>
{
    public string UserId { get; } = userId;
    public string? FullName { get; } = fullName;
    public string? PhoneNumber { get; } = phoneNumber;
}
