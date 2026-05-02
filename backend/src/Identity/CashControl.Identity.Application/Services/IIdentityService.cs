using CashControl.Identity.Application.Services.DTOs;

namespace CashControl.Identity.Application.Services;

public interface IIdentityService
{
    Task RegisterAsync(string email, string password, string? fullName, CancellationToken cancellationToken = default);

    Task LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task RefreshSessionAsync(string userId, CancellationToken cancellationToken = default);

    Task SignOutAsync(string userId, CancellationToken cancellationToken = default);

    Task StopImpersonationAsync(CancellationToken cancellationToken = default);

    Task ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    Task<UserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<UserDto?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(string userId, string? fullName, string? phoneNumber, CancellationToken cancellationToken = default);

    Task AssignRoleAsync(string userId, string role, CancellationToken cancellationToken = default);

    Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default);

    Task<IList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default);

    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    Task ImpersonateAsync(string userId, CancellationToken cancellationToken = default);
}
