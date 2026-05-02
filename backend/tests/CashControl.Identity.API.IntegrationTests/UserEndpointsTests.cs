using System.Net;
using CashControl.Core.API;
using CashControl.Identity.Infra;
using Microsoft.AspNetCore.Mvc.Testing;

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
        var (_, client) = await _factory.RegisterAndLoginAsync("profile@cashcontrol.com", "Pass123", "Profile User");

        var meResponse = await client.GetAsync("/v1/users/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(me);
        Assert.Equal("Profile User", me.FullName);
        Assert.Equal(1, me.Tenant);
        Assert.False(me.CanImpersonateUsers);

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
        Assert.Equal(1, updated.Tenant);
        Assert.False(updated.CanImpersonateUsers);
    }

    [Fact]
    public async Task ChangePassword_ShouldAllowLoginWithNewPassword()
    {
        const string email = "change-password@cashcontrol.com";
        const string password = "Pass123";
        const string newPassword = "Pass456";

        var (_, client) = await _factory.RegisterAndLoginAsync(email, password, "Change Password User");

        var changeResponse = await client.PostAsync("/v1/users/me/change-password", JsonContent.Create(new
        {
            currentPassword = password,
            newPassword
        }));

        Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);

        var loginClient = await _factory.CreateAuthenticatedClientAsync(email, newPassword);
        var meResponse = await loginClient.GetAsync("/v1/users/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
    }

    [Fact]
    public async Task RevokeRefreshToken_ShouldInvalidateCurrentSession()
    {
        var (_, client) = await _factory.RegisterAndLoginAsync("revoke@cashcontrol.com", "Pass123", "Revoke User");

        var revokeResponse = await client.DeleteAsync("/v1/users/me/refresh-token");
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var meResponse = await client.GetAsync("/v1/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    [Fact]
    public async Task MeEndpoint_ShouldRequireAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/v1/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("unauthorized", error.Code);
    }

    [Fact]
    public async Task AuthenticatedWrite_ShouldRejectMissingCsrfToken()
    {
        const string email = "csrf@cashcontrol.com";
        const string password = "Pass123";

        await _factory.RegisterAsync(email, password, "Csrf User");
        await _factory.ConfirmEmailAsync(email);

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var loginResponse = await client.PostAsync("/v1/auth/login", JsonContent.Create(new { email, password }));
        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);

        var updateResponse = await client.PutAsync("/v1/users/me", JsonContent.Create(new
        {
            fullName = "Blocked By Csrf",
            phoneNumber = "5511999999999"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        var error = await updateResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Code);
        Assert.Contains(error.Errors, detail => detail.PropertyName.Equals("Antiforgery", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AdminEndpoint_ShouldRequireAdminRole()
    {
        var (_, client) = await _factory.RegisterAndLoginAsync("member-no-admin@cashcontrol.com", "Pass123", "Member");
        var targetUserId = await _factory.GetUserIdByEmailAsync("member-no-admin@cashcontrol.com");

        var response = await client.GetAsync($"/v1/admin/users/{targetUserId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("forbidden", error.Code);
    }

    [Fact]
    public async Task AdminGetById_ShouldReturnRequestedUser()
    {
        var client = await _factory.CreateAdminClientAsync();
        var (userId, _) = await _factory.RegisterAndLoginAsync("member-get@cashcontrol.com", "Pass123", "Member");

        var getUserResponse = await client.GetAsync($"/v1/admin/users/{userId}");
        Assert.Equal(HttpStatusCode.OK, getUserResponse.StatusCode);

        var user = await getUserResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("Member", user.FullName);
        Assert.Equal(1, user.Tenant);
        Assert.False(user.CanImpersonateUsers);
    }

    [Fact]
    public async Task AdminAssignRole_ShouldPersistRole()
    {
        var client = await _factory.CreateAdminClientAsync();
        var (userId, _) = await _factory.RegisterAndLoginAsync("member-assign@cashcontrol.com", "Pass123", "Member");

        var assignRoleResponse = await client.PutAsync($"/v1/admin/users/{userId}/roles/Manager", content: null);
        Assert.Equal(HttpStatusCode.NoContent, assignRoleResponse.StatusCode);

        var roles = await _factory.GetUserRolesAsync("member-assign@cashcontrol.com");
        Assert.Contains("Manager", roles);
    }

    [Fact]
    public async Task AdminAssignRole_ShouldRejectLegacySuperAdminRole()
    {
        var client = await _factory.CreateAdminClientAsync();
        var (userId, _) = await _factory.RegisterAndLoginAsync("member-legacy-role@cashcontrol.com", "Pass123", "Member");

        var response = await client.PutAsync($"/v1/admin/users/{userId}/roles/{IdentitySeedOptions.SuperAdminRole}", content: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Code);
        Assert.Contains(error.Errors, detail => detail.ErrorMessage.Contains("descontinuada", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AdminGetRoles_ShouldReturnCurrentRoles()
    {
        var client = await _factory.CreateAdminClientAsync();
        var (userId, _) = await _factory.RegisterAndLoginAsync("member-roles@cashcontrol.com", "Pass123", "Member");

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
        var client = await _factory.CreateAdminClientAsync();
        var (userId, _) = await _factory.RegisterAndLoginAsync("member-remove-role@cashcontrol.com", "Pass123", "Member");

        await client.PutAsync($"/v1/admin/users/{userId}/roles/Manager", content: null);

        var removeRoleResponse = await client.DeleteAsync($"/v1/admin/users/{userId}/roles/Manager");
        Assert.Equal(HttpStatusCode.NoContent, removeRoleResponse.StatusCode);

        var roles = await _factory.GetUserRolesAsync("member-remove-role@cashcontrol.com");
        Assert.DoesNotContain("Manager", roles);
    }

    [Fact]
    public async Task AdminDelete_ShouldRemoveUser()
    {
        var client = await _factory.CreateAdminClientAsync();
        var (userId, _) = await _factory.RegisterAndLoginAsync("member-delete@cashcontrol.com", "Pass123", "Member");

        var deleteResponse = await client.DeleteAsync($"/v1/admin/users/{userId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.False(await _factory.UserExistsAsync(userId));
    }

    [Fact]
    public async Task AdminShouldNotManageUserFromAnotherTenant()
    {
        var client = await _factory.CreateAdminClientAsync("tenant-admin@cashcontrol.com", "Admin123");
        var externalUserId = await _factory.CreateUserAsync("tenant-two@cashcontrol.com", "Pass123", "Tenant Two", tenant: 2);

        var response = await client.GetAsync($"/v1/admin/users/{externalUserId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("validation_error", error.Code);
        Assert.Contains(error.Errors, detail => detail.ErrorMessage.Contains("outro tenant", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SuperAdminShouldImpersonateUserFromAnotherTenant()
    {
        var client = await _factory.CreateSuperAdminClientAsync();
        var targetUserId = await _factory.CreateUserAsync("tenant-three@cashcontrol.com", "Pass123", "Tenant Three", tenant: 3, roles: ["Manager"]);

        var response = await client.PostAsync($"/v1/admin/users/{targetUserId}/impersonate", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var meResponse = await client.GetAsync("/v1/users/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(me);
        Assert.Equal(targetUserId, me.Id);
        Assert.Equal(3, me.Tenant);
        Assert.Contains("Manager", me.Roles);

        var adminResponse = await client.GetAsync($"/v1/admin/users/{targetUserId}");
        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task SuperAdminShouldStopImpersonationAndRecoverOriginalSession()
    {
        var client = await _factory.CreateSuperAdminClientAsync();
        var targetUserId = await _factory.CreateUserAsync("tenant-four@cashcontrol.com", "Pass123", "Tenant Four", tenant: 4, roles: ["Manager"]);

        var impersonateResponse = await client.PostAsync($"/v1/admin/users/{targetUserId}/impersonate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, impersonateResponse.StatusCode);

        var csrfRefresh = await client.GetAsync("/v1/auth/csrf-token");
        Assert.Equal(HttpStatusCode.OK, csrfRefresh.StatusCode);
        var csrf = await csrfRefresh.Content.ReadFromJsonAsync<CsrfTokenResponse>();
        Assert.NotNull(csrf);
        client.DefaultRequestHeaders.Remove(csrf.HeaderName ?? "X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add(csrf.HeaderName ?? "X-CSRF-TOKEN", csrf.RequestToken);

        var stopResponse = await client.PostAsync("/v1/users/me/stop-impersonation", content: null);
        Assert.Equal(HttpStatusCode.NoContent, stopResponse.StatusCode);

        var meResponse = await client.GetAsync("/v1/users/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(me);
        Assert.True(me.CanImpersonateUsers);
        Assert.False(me.IsImpersonating);
        Assert.Equal(IdentitySeedOptions.SuperAdminEmail, me.Email);
    }

    [Fact]
    public async Task AdminShouldNotImpersonateUsers()
    {
        var client = await _factory.CreateAdminClientAsync("plain-admin@cashcontrol.com", "Admin123");
        var targetUserId = await _factory.CreateUserAsync("member-impersonation@cashcontrol.com", "Pass123", "Member");

        var response = await client.PostAsync($"/v1/admin/users/{targetUserId}/impersonate", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("forbidden", error.Code);
    }

    private sealed class UserProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int Tenant { get; set; }
        public bool CanImpersonateUsers { get; set; }
        public bool IsImpersonating { get; set; }
        public bool CanStopImpersonation { get; set; }
        public string? ImpersonatedByEmail { get; set; }
        public IList<string> Roles { get; set; } = [];
    }

    private sealed class UserRolesResponse
    {
        public string UserId { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = [];
    }

    private sealed class CsrfTokenResponse
    {
        public string RequestToken { get; set; } = string.Empty;
        public string? HeaderName { get; set; }
    }
}
