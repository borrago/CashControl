using CashControl.Identity.Domain.Entities;
using CashControl.Identity.Infra;
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

            services.AddDbContext<Context>(options => options.UseSqlite(_connection));

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
        var token = await GetEmailConfirmationTokenAsync(email);

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
        var client = CreateClient();

        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "Admin User"
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
                throw new InvalidOperationException(string.Join(", ", create.Errors.Select(x => x.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
            await userManager.AddToRoleAsync(user, "Admin");

        var loginResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password }));
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        ArgumentNullException.ThrowIfNull(login);

        return login.AccessToken!;
    }

    public async Task<string> GetPasswordResetTokenAsync(string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        ArgumentNullException.ThrowIfNull(user);

        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<string> GetEmailConfirmationTokenAsync(string email)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);
        ArgumentNullException.ThrowIfNull(user);

        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

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
}
