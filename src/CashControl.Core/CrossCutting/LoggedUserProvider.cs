using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CashControl.Core.CrossCutting;

public class LoggedUserProvider : ILoggedUserProvider
{
    public Guid? IdUsuario { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public int Tenant { get; private set; }

    public LoggedUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor?.HttpContext ?? new DefaultHttpContext();
        if (httpContext.User.Identity is not ClaimsIdentity claim || !claim.IsAuthenticated)
            return;

        WithClaimsIdentity(claim);
    }

    public LoggedUserProvider(ClaimsIdentity claims)
    {
        WithClaimsIdentity(claims);
    }

    public bool IsLogged()
    {
        return IdUsuario != null && IdUsuario != Guid.Empty;
    }

    private void WithClaimsIdentity(ClaimsIdentity claim)
    {
        if (Guid.TryParse(claim.FindFirst(ClaimTypes.Upn)?.Value, out var userPrincipalName))
            IdUsuario = userPrincipalName;

        Nome = claim.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        if (int.TryParse(claim.FindFirst("Tenant")?.Value, out var tenant))
            Tenant = tenant;
    }
}
