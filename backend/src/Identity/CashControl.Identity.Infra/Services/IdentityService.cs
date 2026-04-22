using CashControl.Core.Application;
using CashControl.Core.CrossCutting;
using CashControl.Identity.Application.Services;
using CashControl.Identity.Application.Services.DTOs;
using CashControl.Identity.Domain.Entities;
using CashControl.Identity.Infra.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoreApplicationException = CashControl.Core.Application.ApplicationException;

namespace CashControl.Identity.Infra.Services;

public class IdentityService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<JwtOptions> jwtOptions,
    IOptions<EmailOptions> emailOptions,
    IIdentityEmailSender identityEmailSender) : IIdentityService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
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
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw CreateApplicationException(result);

        await SendConfirmationEmailAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
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

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
            throw new CoreApplicationException("Token invalido.");

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new CoreApplicationException("Usuario nao encontrado.");

        if (!user.EmailConfirmed)
            throw new CoreApplicationException("E-mail nao confirmado.");

        if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTimeUtc is null || user.RefreshTokenExpiryTimeUtc <= DateTime.UtcNow)
            throw new CoreApplicationException("Refresh token invalido ou expirado.");

        return await IssueTokensAsync(user);
    }

    public async Task RevokeRefreshTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        user.RevokeRefreshToken();

        await UpdateUserAsync(user);
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
    }

    public async Task<UserDto?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        return await MapUserAsync(user);
    }

    public Task<UserDto?> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
        => GetByIdAsync(userId, cancellationToken);

    public async Task UpdateProfileAsync(string userId, string? fullName, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        user.UpdateProfile(fullName, phoneNumber);

        await UpdateUserAsync(user);
    }

    public async Task AssignRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var normalizedRole = role.Trim();
        var user = await GetRequiredUserAsync(userId);

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
        var user = await GetRequiredUserAsync(userId);
        var result = await _userManager.RemoveFromRoleAsync(user, role.Trim());

        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    public async Task<IList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        return await _userManager.GetRolesAsync(user);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId);
        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user)
    {
        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        user.SetRefreshToken(refreshToken, refreshTokenExpiresAtUtc);
        await UpdateUserAsync(user);

        return new AuthResponseDto(accessToken, refreshToken, refreshTokenExpiresAtUtc);
    }

    private async Task<User> GetRequiredUserAsync(string userId)
        => await _userManager.FindByIdAsync(userId) ?? throw new CoreApplicationException("Usuario nao encontrado.");

    private async Task<UserDto> MapUserAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Roles = roles
        };
    }

    private async Task UpdateUserAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw CreateApplicationException(result);
    }

    private async Task<string> GenerateAccessTokenAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(roleValue => new Claim(ClaimTypes.Role, roleValue)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Token invalido.");
        }

        return principal;
    }

    private static CoreApplicationException CreateApplicationException(IdentityResult result)
        => new(result.Errors.Select(error => new CustomValidationFailure(error.Code, error.Description)));

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
}
