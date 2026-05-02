using CashControl.Core.CrossCutting;
using CashControl.Identity.Application.Services;
using CashControl.Identity.Application.Services.DTOs;
using CashControl.Identity.Domain.Entities;
using CashControl.Identity.Infra.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CoreApplicationException = CashControl.Core.Application.ApplicationException;

namespace CashControl.Identity.Infra.Services;

public class IdentityService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<IdentityRole> roleManager,
    IHttpContextAccessor httpContextAccessor,
    IOptions<EmailOptions> emailOptions,
    IIdentityEmailSender identityEmailSender) : IIdentityService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly EmailOptions _emailOptions = emailOptions.Value;
    private readonly IIdentityEmailSender _identityEmailSender = identityEmailSender;

    public async Task RegisterAsync(string email, string password, string? fullName, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
            throw new CoreApplicationException("Ja existe um usuario com este e-mail.");

        var user = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = fullName?.Trim(),
            EmailConfirmed = false,
            Tenant = User.DefaultTenant,
            IsSuperUser = false
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw CreateApplicationException(result);

        await SendConfirmationEmailAsync(user, cancellationToken);
    }

    public async Task LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail)
            ?? throw new CoreApplicationException("Usuario nao encontrado.");

        if (!user.EmailConfirmed)
            throw new CoreApplicationException("E-mail nao confirmado.");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (signInResult.IsLockedOut)
            throw new CoreApplicationException("Usuario temporariamente bloqueado por excesso de tentativas. Tente novamente mais tarde.");

        if (!signInResult.Succeeded)
            throw new CoreApplicationException("Usuario ou senha invalidos.");

        await _signInManager.SignInAsync(user, isPersistent: false);
    }

    public async Task RefreshSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        await RefreshCurrentSessionAsync(user);
    }

    public async Task SignOutAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        user.RevokeRefreshToken();
        await UpdateUserAsync(user);
        await _signInManager.SignOutAsync();
    }

    public async Task StopImpersonationAsync(CancellationToken cancellationToken = default)
    {
        var principal = GetRequiredPrincipal();
        var originalUserId = principal.FindFirstValue(CustomClaimTypes.ImpersonatedByUserId);

        if (string.IsNullOrWhiteSpace(originalUserId))
            throw new CoreApplicationException("A sessao atual nao esta em modo de impersonacao.");

        var originalUser = await GetRequiredUserAsync(originalUserId);
        if (!originalUser.IsSuperUser)
            throw new CoreApplicationException("A sessao original nao possui privilegios para retomar a impersonacao.");

        await _signInManager.SignInAsync(originalUser, isPersistent: false);
    }

    public async Task ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null || !user.EmailConfirmed)
            return;

        await SendPasswordResetEmailAsync(user, cancellationToken);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim())
            ?? throw new CoreApplicationException("Usuario nao encontrado.");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
            throw CreateApplicationException(result);

        await RefreshCurrentSessionAsync(user);
    }

    public async Task<UserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetAccessibleUserAsync(userId);
        return await MapUserAsync(user);
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        return await MapUserAsync(user, GetRequiredPrincipal());
    }

    public async Task UpdateProfileAsync(string userId, string? fullName, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        user.UpdateProfile(fullName, phoneNumber);

        await UpdateUserAsync(user);
        await RefreshCurrentSessionAsync(user);
    }

    public async Task AssignRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var normalizedRole = role.Trim();
        var user = await GetAccessibleUserAsync(userId);
        EnsureNotReservedSystemRole(normalizedRole);

        if (!await _roleManager.RoleExistsAsync(normalizedRole))
        {
            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(normalizedRole));
            if (!createRoleResult.Succeeded)
                throw CreateApplicationException(createRoleResult);
        }

        if (await _userManager.IsInRoleAsync(user, normalizedRole))
            return;

        var result = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    public async Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var normalizedRole = role.Trim();
        var user = await GetAccessibleUserAsync(userId);
        EnsureNotReservedSystemRole(normalizedRole);

        var result = await _userManager.RemoveFromRoleAsync(user, normalizedRole);
        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    public async Task<IList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetAccessibleUserAsync(userId);
        return await _userManager.GetRolesAsync(user);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var actor = GetRequiredActor();
        var user = await GetAccessibleUserAsync(userId);

        if (user.IsSuperUser && !actor.IsSuperUser)
            throw new CoreApplicationException("Somente o super usuario pode remover outro super usuario.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    public async Task ImpersonateAsync(string userId, CancellationToken cancellationToken = default)
    {
        var actor = GetRequiredActor();
        if (actor.IsImpersonating)
            throw new CoreApplicationException("Nao e permitido iniciar uma nova impersonacao antes de encerrar a sessao atual.");

        if (!actor.IsSuperUser)
            throw new CoreApplicationException("Somente o super usuario pode impersonar usuarios.");

        var targetUser = await GetRequiredUserAsync(userId);
        if (targetUser.IsSuperUser)
            throw new CoreApplicationException("Nao e permitido impersonar outro super usuario.");

        var claims = new List<Claim>
        {
            new(CustomClaimTypes.IsImpersonating, bool.TrueString.ToLowerInvariant()),
            new(CustomClaimTypes.ImpersonatedByUserId, actor.UserId),
            new(CustomClaimTypes.ImpersonatedByEmail, actor.Email ?? string.Empty)
        };

        await _signInManager.SignInWithClaimsAsync(targetUser, isPersistent: false, claims);
    }

    private async Task<User> GetRequiredUserAsync(string userId)
        => await _userManager.FindByIdAsync(userId) ?? throw new CoreApplicationException("Usuario nao encontrado.");

    private async Task<User> GetAccessibleUserAsync(string userId)
    {
        var actor = GetRequiredActor();
        var user = await GetRequiredUserAsync(userId);

        if (actor.IsSuperUser)
            return user;

        if (user.IsSuperUser)
            throw new CoreApplicationException("O super usuario nao pode ser administrado por este tenant.");

        if (actor.Tenant == 0 || actor.Tenant != user.Tenant)
            throw new CoreApplicationException("Operacao nao permitida para usuarios de outro tenant.");

        return user;
    }

    private async Task<UserDto> MapUserAsync(User user, ClaimsPrincipal? principal = null)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var isImpersonating = bool.TryParse(principal?.FindFirstValue(CustomClaimTypes.IsImpersonating), out var parsedIsImpersonating) && parsedIsImpersonating;
        var impersonatedByEmail = principal?.FindFirstValue(CustomClaimTypes.ImpersonatedByEmail);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Tenant = user.Tenant,
            CanImpersonateUsers = GetRequiredActor().IsSuperUser && !isImpersonating,
            IsImpersonating = isImpersonating,
            CanStopImpersonation = isImpersonating && !string.IsNullOrWhiteSpace(principal?.FindFirstValue(CustomClaimTypes.ImpersonatedByUserId)),
            ImpersonatedByEmail = impersonatedByEmail,
            Roles = roles
        };
    }

    private async Task UpdateUserAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    private static CoreApplicationException CreateApplicationException(IdentityResult result)
        => new(result.Errors.Select(error => new CustomValidationFailure(error.Code, error.Description)));

    private static void EnsureNotReservedSystemRole(string normalizedRole)
    {
        if (normalizedRole.Equals(IdentitySeedOptions.SuperAdminRole, StringComparison.OrdinalIgnoreCase))
            throw new CoreApplicationException("A role SuperAdmin esta descontinuada. O privilegio global agora e controlado exclusivamente por IsSuperUser.");
    }

    private ClaimsPrincipal GetRequiredPrincipal()
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true
            ? _httpContextAccessor.HttpContext.User
            : throw new CoreApplicationException("Usuario autenticado nao encontrado.");

    private RequestActorContext GetRequiredActor()
    {
        var principal = GetRequiredPrincipal();
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new CoreApplicationException("Usuario autenticado sem identificador.");

        var email = principal.FindFirstValue(ClaimTypes.Email);
        var isSuperUser = bool.TryParse(principal.FindFirstValue(CustomClaimTypes.IsSuperUser), out var parsedIsSuperUser) && parsedIsSuperUser;
        var tenant = int.TryParse(principal.FindFirstValue(CustomClaimTypes.Tenant), out var parsedTenant) ? parsedTenant : 0;
        var isImpersonating = bool.TryParse(principal.FindFirstValue(CustomClaimTypes.IsImpersonating), out var parsedIsImpersonating) && parsedIsImpersonating;

        return new RequestActorContext(userId, email, tenant, isSuperUser, isImpersonating);
    }

    private async Task RefreshCurrentSessionAsync(User user)
    {
        var principal = GetRequiredPrincipal();
        var isImpersonating = bool.TryParse(principal.FindFirstValue(CustomClaimTypes.IsImpersonating), out var parsedIsImpersonating) && parsedIsImpersonating;

        if (!isImpersonating)
        {
            await _signInManager.RefreshSignInAsync(user);
            return;
        }

        var claims = new List<Claim>
        {
            new(CustomClaimTypes.IsImpersonating, bool.TrueString.ToLowerInvariant()),
            new(CustomClaimTypes.ImpersonatedByUserId, principal.FindFirstValue(CustomClaimTypes.ImpersonatedByUserId) ?? string.Empty),
            new(CustomClaimTypes.ImpersonatedByEmail, principal.FindFirstValue(CustomClaimTypes.ImpersonatedByEmail) ?? string.Empty)
        };

        await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, claims);
    }

    private async Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationUrl = BuildActionUrl(
            _emailOptions.ConfirmationUrl,
            new KeyValuePair<string, string>("userId", user.Id),
            new KeyValuePair<string, string>("token", token));

        var body = $"""
            Ola {user.FullName ?? user.Email},

            Confirme seu e-mail para ativar o acesso ao CashControl.
            Link: {confirmationUrl}
            """;

        await _identityEmailSender.SendAsync(
            new IdentityEmailMessage(user.Email ?? string.Empty, "Confirmacao de e-mail", body),
            cancellationToken);
    }

    private async Task SendPasswordResetEmailAsync(User user, CancellationToken cancellationToken)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = BuildActionUrl(
            _emailOptions.ResetPasswordUrl,
            new KeyValuePair<string, string>("email", user.Email ?? string.Empty),
            new KeyValuePair<string, string>("token", token));

        var body = $"""
            Ola {user.FullName ?? user.Email},

            Recebemos uma solicitacao para redefinir sua senha.
            Link: {resetUrl}
            """;

        await _identityEmailSender.SendAsync(
            new IdentityEmailMessage(user.Email ?? string.Empty, "Redefinicao de senha", body),
            cancellationToken);
    }

    private static string BuildActionUrl(string? baseUrl, params KeyValuePair<string, string>[] values)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return string.Join(System.Environment.NewLine, values.Select(value => $"{value.Key}: {value.Value}"));

        var queryString = string.Join(
            "&",
            values.Select(value => $"{Uri.EscapeDataString(value.Key)}={Uri.EscapeDataString(value.Value)}"));

        var separator = baseUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return $"{baseUrl}{separator}{queryString}";
    }

    private sealed record RequestActorContext(string UserId, string? Email, int Tenant, bool IsSuperUser, bool IsImpersonating);
}
