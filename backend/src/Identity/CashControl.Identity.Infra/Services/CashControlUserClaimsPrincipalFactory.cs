using CashControl.Core.CrossCutting;
using CashControl.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CashControl.Identity.Infra.Services;

public class CashControlUserClaimsPrincipalFactory(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<User, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(CustomClaimTypes.Tenant, user.Tenant.ToString()));
        identity.AddClaim(new Claim(CustomClaimTypes.IsSuperUser, user.IsSuperUser.ToString().ToLowerInvariant()));
        return identity;
    }
}
