using CashControl.Identity.Domain.Entities;
using CashControl.Identity.Application.Services;
using CashControl.Identity.Infra;
using CashControl.Identity.Infra.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CashControl.Identity.API.IntegrationTests;

public class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<Context>>();
            services.RemoveAll<Context>();
            services.RemoveAll<IIdentityEmailSender>();

            services.AddDbContext<Context>(options => options.UseSqlite(_connection));
            services.AddSingleton<TestIdentityEmailSender>();
            services.AddSingleton<IIdentityEmailSender>(sp => sp.GetRequiredService<TestIdentityEmailSender>());

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Context>();
            context.Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _connection.DisposeAsync();
        Dispose();
    }

    public async Task RegisterAsync(string email, string password, string? fullName = null)
    {
        var client = CreateClient();
        var registerResponse = await client.PostAsync("/v1/auth/register", JsonContent.Create(new { email, password, fullName }));
        registerResponse.EnsureSuccessStatusCode();
    }

    public async Task ConfirmEmailAsync(string email)
    {
        var client = CreateClient();
        var userId = await GetUserIdByEmailAsync(email);
        var token = await GetConfirmationTokenFromSentEmailAsync(email);

        var confirmResponse = await client.PostAsync("/v1/auth/confirm-email", JsonContent.Create(new { userId, token }));
        confirmResponse.EnsureSuccessStatusCode();
    }

    public async Task<(string UserId, string AccessToken, string RefreshToken)> RegisterAndLoginAsync(string email, string password, string? fullName = null)
    {
        await RegisterAsync(email, password, fullName);
        await ConfirmEmailAsync(email);

        var client = CreateClient();
        var loginResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password }));
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        ArgumentNullException.ThrowIfNull(login);

        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        ArgumentNullException.ThrowIfNull(user);

        return (user.Id, login.AccessToken!, login.RefreshToken!);
    }

    public HttpClient CreateAuthenticatedClient(string accessToken)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public async Task<string> CreateAdminAccessTokenAsync(string email = "admin@cashcontrol.com", string password = "Admin123")
    {
        await EnsureUserAsync(email, password, "Admin User", tenant: User.DefaultTenant, isSuperUser: false, roles: [IdentitySeedOptions.AdminRole]);
        return await LoginAndGetAccessTokenAsync(email, password);
    }

    public Task<string> CreateSuperAdminAccessTokenAsync(
        string email = IdentitySeedOptions.SuperAdminEmail,
        string password = IdentitySeedOptions.SuperAdminPassword)
        => LoginAndGetAccessTokenAsync(email, password);

    public async Task<string> CreateUserAsync(
        string email,
        string password,
        string? fullName = null,
        int tenant = User.DefaultTenant,
        bool isSuperUser = false,
        params string[] roles)
    {
        await EnsureUserAsync(email, password, fullName, tenant, isSuperUser, roles);
        return await GetUserIdByEmailAsync(email);
    }

    public async Task<string> GetPasswordResetTokenAsync(string email)
        => await GetTokenFromSentEmailAsync(email, "Redefinicao de senha");

    public async Task<string> GetEmailConfirmationTokenAsync(string email)
        => await GetTokenFromSentEmailAsync(email, "Confirmacao de e-mail");

    public async Task<string> GetUserIdByEmailAsync(string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        ArgumentNullException.ThrowIfNull(user);

        return user.Id;
    }

    public async Task<bool> IsEmailConfirmedAsync(string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        ArgumentNullException.ThrowIfNull(user);

        return user.EmailConfirmed;
    }

    public async Task<IList<string>> GetUserRolesAsync(string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        ArgumentNullException.ThrowIfNull(user);

        return await userManager.GetRolesAsync(user);
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(userId);
        return user is not null;
    }

    public sealed class AuthTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    }

    public async Task<string> GetConfirmationTokenFromSentEmailAsync(string email)
        => await GetTokenFromSentEmailAsync(email, "Confirmacao de e-mail");

    private async Task<string> LoginAndGetAccessTokenAsync(string email, string password)
    {
        var client = CreateClient();
        var loginResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password }));
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        ArgumentNullException.ThrowIfNull(login);

        return login.AccessToken!;
    }

    private async Task EnsureUserAsync(
        string email,
        string password,
        string? fullName,
        int tenant,
        bool isSuperUser,
        params string[] roles)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                Tenant = tenant,
                IsSuperUser = isSuperUser
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
                throw new InvalidOperationException(string.Join(", ", create.Errors.Select(x => x.Description)));
        }
        else
        {
            user.UserName = email;
            user.Email = email;
            user.EmailConfirmed = true;
            user.FullName = fullName;
            user.Tenant = tenant;
            user.IsSuperUser = isSuperUser;

            var update = await userManager.UpdateAsync(user);
            if (!update.Succeeded)
                throw new InvalidOperationException(string.Join(", ", update.Errors.Select(x => x.Description)));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var missingRoles = roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(role => !currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (missingRoles.Length > 0)
        {
            var addRoles = await userManager.AddToRolesAsync(user, missingRoles);
            if (!addRoles.Succeeded)
                throw new InvalidOperationException(string.Join(", ", addRoles.Errors.Select(x => x.Description)));
        }
    }

    private Task<string> GetTokenFromSentEmailAsync(string email, string subject)
    {
        using var scope = Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<TestIdentityEmailSender>();
        var message = sender.GetLastMessage(email, subject);
        ArgumentNullException.ThrowIfNull(message);

        var token = ExtractQueryParameter(message.Body, "token");
        return Task.FromResult(token);
    }

    private static string ExtractQueryParameter(string content, string parameterName)
    {
        var marker = $"{parameterName}=";
        var start = content.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
            throw new InvalidOperationException($"Parametro '{parameterName}' nao encontrado no e-mail.");

        start += marker.Length;
        var end = content.IndexOfAny(['&', '\r', '\n'], start);
        var encodedValue = end >= 0 ? content[start..end] : content[start..];
        return Uri.UnescapeDataString(encodedValue.Trim());
    }
}
