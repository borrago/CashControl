using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
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
        if (httpContext?.User.Identity is not ClaimsIdentity claim || !claim.IsAuthenticated)
            return;

        WithClaimsIdentity(claim);
    }

    public LoggedUserProvider(ClaimsIdentity claims)
    {
        WithClaimsIdentity(claims);
    }

    public void SetAuthorizationFromContext(HttpContext httpContext)
    {
        if (httpContext != null)
        {
            var tokenFromAuthorization = GetTokenFromAuthorization(httpContext);
            if (tokenFromAuthorization != null)
            {
                SetClaimsFromAuthorization(tokenFromAuthorization);
                return;
            }
        }
    }

    public bool IsLogged()
    {
        return IdUsuario != null && IdUsuario != Guid.Empty;
    }

    private void WithClaimsIdentity(ClaimsIdentity claim)
    {
        if (claim == null) return;

        var userPrincipalName = Guid.Parse(claim.FindFirst(ClaimTypes.Upn)?.Value ?? "");
        IdUsuario = userPrincipalName;

        var name = claim.FindFirst(ClaimTypes.Name)?.Value ?? "";
        Nome = name;

        var tenant = int.Parse(claim.FindFirst("Tenant")?.Value ?? "");
        Tenant = tenant;
    }

    private void SetClaimsFromAuthorization(string tokenId)
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenS = handler.ReadJwtToken(tokenId);

        WithClaimsIdentity(new ClaimsIdentity(tokenS.Claims));
    }

    private static string GetTokenFromAuthorization(HttpContext context)
    {
        if (!string.IsNullOrEmpty(context.Request.Query["Authorization"].ToString()))
            return context.Request.Query["Authorization"].ToString();

        return context.Request.Headers["Authorization"].ToString();
    }
}