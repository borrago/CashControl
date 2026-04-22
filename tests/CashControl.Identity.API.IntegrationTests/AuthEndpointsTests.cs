using System.Net;

namespace CashControl.Identity.API.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<IdentityApiFactory>
{
    private readonly IdentityApiFactory _factory;

    public AuthEndpointsTests(IdentityApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ShouldReturnTokens()
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

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var register = await registerResponse.Content.ReadFromJsonAsync<IdentityApiFactory.AuthTokenResponse>();
        Assert.NotNull(register);
        Assert.False(string.IsNullOrWhiteSpace(register.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(register.RefreshToken));
    }

    [Fact]
    public async Task Login_ShouldReturnTokensForExistingUser()
    {
        var client = _factory.CreateClient();
        const string email = "login@cashcontrol.com";
        const string password = "Pass123";

        await client.PostAsync("/v1/auth/register", JsonContent.Create(new { email, password, fullName = "Login User" }));

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

        await client.PostAsync("/v1/auth/register", JsonContent.Create(new { email, password, fullName = "Refresh User" }));

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
    public async Task ForgotPassword_ShouldReturnResetToken()
    {
        var client = _factory.CreateClient();
        const string email = "reset@cashcontrol.com";
        const string password = "Pass123";

        await client.PostAsync("/v1/auth/register", JsonContent.Create(new { email, password, fullName = "Reset User" }));

        var forgotResponse = await client.PostAsync("/v1/auth/forgot-password", JsonContent.Create(new { email }));
        Assert.Equal(HttpStatusCode.OK, forgotResponse.StatusCode);

        var forgotPayload = await forgotResponse.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(forgotPayload);
        Assert.False(string.IsNullOrWhiteSpace(forgotPayload.Token));
    }

    [Fact]
    public async Task ResetPassword_ShouldAllowLoginWithNewPassword()
    {
        var client = _factory.CreateClient();
        const string email = "reset-password@cashcontrol.com";
        const string password = "Pass123";
        const string newPassword = "Pass456";

        await client.PostAsync("/v1/auth/register", JsonContent.Create(new { email, password, fullName = "Reset Password User" }));

        var token = await _factory.GetPasswordResetTokenAsync(email);

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

    private sealed class ForgotPasswordResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
