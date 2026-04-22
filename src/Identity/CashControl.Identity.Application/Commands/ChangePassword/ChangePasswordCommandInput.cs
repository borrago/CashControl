using CashControl.Core.Application;

namespace CashControl.Identity.Application.Commands.ChangePassword;

public class ChangePasswordCommandInput(string userName, string passWord, string newPassword) : CommandInput<ChangePasswordCommandResult>
{
    public string UserId { get; } = userName;
    public string CurrentPassword { get; } = passWord;
    public string NewPassword { get; } = newPassword;
}
