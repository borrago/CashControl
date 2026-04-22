using System.Net;
using CashControl.Core.API;

namespace CashControl.Identity.API.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<IdentityApiFactory>
{
    private readonly IdentityApiFactory _factory;

    public AuthEndpointsTests(IdentityApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ShouldRequireEmailConfirmationBeforeLogin()
    {
        var client = _factory.CreateClient();
        const string email = "john@cashcontrol.com";
        const string password = "Pass123";

        var registerResponse = await client.PostAsync("/v1/auth/register", JsonContent.Create(new
        {
            email,
            password,
            fullName = "John Doe"
        }));

        Assert.Equal(HttpStatusCode.NoContent, registerResponse.StatusCode);

        var loginResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password }));
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);

        var error = await loginResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Code);
    }

    [Fact]
    public async Task Login_ShouldReturnTokensForConfirmedUser()
    {
        const string email = "login@cashcontrol.com";
        const string password = "Pass123";

        await _factory.RegisterAsync(email, password, "Login User");
        await _factory.ConfirmEmailAsync(email);

        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password }));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var login = await loginResponse.Content.ReadFromJsonAsync<IdentityApiFactory.AuthTokenResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));
    }

    [Fact]
    public async Task RefreshToken_ShouldIssueNewTokens()
    {
        var client = _factory.CreateClient();
        const string email = "refresh@cashcontrol.com";
        const string password = "Pass123";

        await _factory.RegisterAsync(email, password, "Refresh User");
        await _factory.ConfirmEmailAsync(email);

        var loginResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password }));
        var login = await loginResponse.Content.ReadFromJsonAsync<IdentityApiFactory.AuthTokenResponse>();
        Assert.NotNull(login);

        var refreshResponse = await client.PostAsync("/v1/auth/refresh-token", JsonContent.Create(new
        {
            accessToken = login.AccessToken,
            refreshToken = login.RefreshToken
        }));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refresh = await refreshResponse.Content.ReadFromJsonAsync<IdentityApiFactory.AuthTokenResponse>();
        Assert.NotNull(refresh);
        Assert.NotEqual(login.AccessToken, refresh.AccessToken);
        Assert.NotEqual(login.RefreshToken, refresh.RefreshToken);
    }

    [Fact]
    public async Task ForgotPassword_ShouldNotExposeResetToken()
    {
        var client = _factory.CreateClient();
        const string email = "reset@cashcontrol.com";
        const string password = "Pass123";

        await _factory.RegisterAsync(email, password, "Reset User");
        await _factory.ConfirmEmailAsync(email);

        var forgotResponse = await client.PostAsync("/v1/auth/forgot-password", JsonContent.Create(new { email }));
        Assert.Equal(HttpStatusCode.NoContent, forgotResponse.StatusCode);
        Assert.Empty(await forgotResponse.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Register_ShouldSendConfirmationEmail()
    {
        var client = _factory.CreateClient();
        const string email = "confirm-email@cashcontrol.com";
        const string password = "Pass123";

        var registerResponse = await client.PostAsync("/v1/auth/register", JsonContent.Create(new
        {
            email,
            password,
            fullName = "Confirm Email User"
        }));

        Assert.Equal(HttpStatusCode.NoContent, registerResponse.StatusCode);

        var token = await _factory.GetConfirmationTokenFromSentEmailAsync(email);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task ResetPassword_ShouldAllowLoginWithNewPassword()
    {
        const string email = "reset-password@cashcontrol.com";
        const string password = "Pass123";
        const string newPassword = "Pass456";

        await _factory.RegisterAsync(email, password, "Reset Password User");
        await _factory.ConfirmEmailAsync(email);

        var forgotClient = _factory.CreateClient();
        var forgotResponse = await forgotClient.PostAsync("/v1/auth/forgot-password", JsonContent.Create(new { email }));
        Assert.Equal(HttpStatusCode.NoContent, forgotResponse.StatusCode);

        var token = await _factory.GetPasswordResetTokenAsync(email);

        var client = _factory.CreateClient();
        var resetResponse = await client.PostAsync("/v1/auth/reset-password", JsonContent.Create(new
        {
            email,
            token,
            newPassword
        }));

        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        var loginResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password = newPassword }));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldMarkUserAsConfirmed()
    {
        var client = _factory.CreateClient();
        const string email = "confirm@cashcontrol.com";
        const string password = "Pass123";

        await client.PostAsync("/v1/auth/register", JsonContent.Create(new { email, password, fullName = "Confirm User" }));

        var userId = await _factory.GetUserIdByEmailAsync(email);
        var token = await _factory.GetEmailConfirmationTokenAsync(email);

        var confirmResponse = await client.PostAsync("/v1/auth/confirm-email", JsonContent.Create(new
        {
            userId,
            token
        }));

        Assert.Equal(HttpStatusCode.NoContent, confirmResponse.StatusCode);
        Assert.True(await _factory.IsEmailConfirmedAsync(email));
    }

    [Fact]
    public async Task Login_ShouldLockUserAfterRepeatedInvalidAttempts()
    {
        var client = _factory.CreateClient();
        const string email = "lockout@cashcontrol.com";
        const string password = "Pass123";

        await _factory.RegisterAsync(email, password, "Lockout User");
        await _factory.ConfirmEmailAsync(email);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var invalidResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password = "Wrong123" }));
            Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        }

        var lockedResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password }));
        Assert.Equal(HttpStatusCode.BadRequest, lockedResponse.StatusCode);

        var error = await lockedResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains(error.Errors, detail => detail.ErrorMessage.Contains("bloqueado", StringComparison.OrdinalIgnoreCase));
    }
}
