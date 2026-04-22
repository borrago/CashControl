using System.Net;
using System.Net.Http.Headers;

namespace CashControl.Identity.API.IntegrationTests;

public class UserEndpointsTests : IClassFixture<IdentityApiFactory>
{
    private readonly IdentityApiFactory _factory;

    public UserEndpointsTests(IdentityApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MeEndpoints_ShouldReturnAndUpdateCurrentUser()
    {
        var (_, accessToken, _) = await _factory.RegisterAndLoginAsync("profile@cashcontrol.com", "Pass123", "Profile User");
        var client = _factory.CreateAuthenticatedClient(accessToken);

        var meResponse = await client.GetAsync("/v1/users/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(me);
        Assert.Equal("Profile User", me.FullName);

        var updateResponse = await client.PutAsync("/v1/users/me", JsonContent.Create(new
        {
            fullName = "Updated Name",
            phoneNumber = "5511988887777"
        }));

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var updatedMeResponse = await client.GetAsync("/v1/users/me");
        var updated = await updatedMeResponse.Content.ReadFromJsonAsync<UserProfileResponse>();

        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.FullName);
        Assert.Equal("5511988887777", updated.PhoneNumber);
    }

    [Fact]
    public async Task ChangePassword_ShouldAllowLoginWithNewPassword()
    {
        const string email = "change-password@cashcontrol.com";
        const string password = "Pass123";
        const string newPassword = "Pass456";

        var (_, accessToken, _) = await _factory.RegisterAndLoginAsync(email, password, "Change Password User");
        var client = _factory.CreateAuthenticatedClient(accessToken);

        var changeResponse = await client.PostAsync("/v1/users/me/change-password", JsonContent.Create(new
        {
            currentPassword = password,
            newPassword
        }));

        Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);

        var loginClient = _factory.CreateClient();
        var loginResponse = await loginClient.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password = newPassword }));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task RevokeRefreshToken_ShouldInvalidateCurrentRefreshToken()
    {
        var (_, accessToken, refreshToken) = await _factory.RegisterAndLoginAsync("revoke@cashcontrol.com", "Pass123", "Revoke User");
        var client = _factory.CreateAuthenticatedClient(accessToken);

        var revokeResponse = await client.DeleteAsync("/v1/users/me/refresh-token");
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var refreshClient = _factory.CreateClient();
        var refreshResponse = await refreshClient.PostAsync("/v1/auth/refresh-token", JsonContent.Create(new
        {
            accessToken,
            refreshToken
        }));

        Assert.Equal(HttpStatusCode.BadRequest, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task MeEndpoint_ShouldRequireAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/v1/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_ShouldRequireAdminRole()
    {
        var (_, accessToken, _) = await _factory.RegisterAndLoginAsync("member-no-admin@cashcontrol.com", "Pass123", "Member");
        var client = _factory.CreateAuthenticatedClient(accessToken);
        var targetUserId = await _factory.GetUserIdByEmailAsync("member-no-admin@cashcontrol.com");

        var response = await client.GetAsync($"/v1/admin/users/{targetUserId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminGetById_ShouldReturnRequestedUser()
    {
        var client = _factory.CreateAuthenticatedClient(await _factory.CreateAdminAccessTokenAsync());
        var (userId, _, _) = await _factory.RegisterAndLoginAsync("member-get@cashcontrol.com", "Pass123", "Member");

        var getUserResponse = await client.GetAsync($"/v1/admin/users/{userId}");
        Assert.Equal(HttpStatusCode.OK, getUserResponse.StatusCode);

        var user = await getUserResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("Member", user.FullName);
    }

    [Fact]
    public async Task AdminAssignRole_ShouldPersistRole()
    {
        var client = _factory.CreateAuthenticatedClient(await _factory.CreateAdminAccessTokenAsync());
        var (userId, _, _) = await _factory.RegisterAndLoginAsync("member-assign@cashcontrol.com", "Pass123", "Member");

        var assignRoleResponse = await client.PutAsync($"/v1/admin/users/{userId}/roles/Manager", content: null);
        Assert.Equal(HttpStatusCode.NoContent, assignRoleResponse.StatusCode);

        var roles = await _factory.GetUserRolesAsync("member-assign@cashcontrol.com");
        Assert.Contains("Manager", roles);
    }

    [Fact]
    public async Task AdminGetRoles_ShouldReturnCurrentRoles()
    {
        var adminToken = await _factory.CreateAdminAccessTokenAsync();
        var client = _factory.CreateAuthenticatedClient(adminToken);
        var (userId, _, _) = await _factory.RegisterAndLoginAsync("member-roles@cashcontrol.com", "Pass123", "Member");

        await client.PutAsync($"/v1/admin/users/{userId}/roles/Manager", content: null);

        var getRolesResponse = await client.GetAsync($"/v1/admin/users/{userId}/roles");
        Assert.Equal(HttpStatusCode.OK, getRolesResponse.StatusCode);

        var roles = await getRolesResponse.Content.ReadFromJsonAsync<UserRolesResponse>();
        Assert.NotNull(roles);
        Assert.Contains("Manager", roles.Roles);
    }

    [Fact]
    public async Task AdminRemoveRole_ShouldDeleteAssignedRole()
    {
        var client = _factory.CreateAuthenticatedClient(await _factory.CreateAdminAccessTokenAsync());
        var (userId, _, _) = await _factory.RegisterAndLoginAsync("member-remove-role@cashcontrol.com", "Pass123", "Member");

        await client.PutAsync($"/v1/admin/users/{userId}/roles/Manager", content: null);

        var removeRoleResponse = await client.DeleteAsync($"/v1/admin/users/{userId}/roles/Manager");
        Assert.Equal(HttpStatusCode.NoContent, removeRoleResponse.StatusCode);

        var roles = await _factory.GetUserRolesAsync("member-remove-role@cashcontrol.com");
        Assert.DoesNotContain("Manager", roles);
    }

    [Fact]
    public async Task AdminDelete_ShouldRemoveUser()
    {
        var client = _factory.CreateAuthenticatedClient(await _factory.CreateAdminAccessTokenAsync());
        var (userId, _, _) = await _factory.RegisterAndLoginAsync("member-delete@cashcontrol.com", "Pass123", "Member");

        var deleteResponse = await client.DeleteAsync($"/v1/admin/users/{userId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.False(await _factory.UserExistsAsync(userId));
    }

    private sealed class UserProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public IList<string> Roles { get; set; } = [];
    }

    private sealed class UserRolesResponse
    {
        public string UserId { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = [];
    }
}
