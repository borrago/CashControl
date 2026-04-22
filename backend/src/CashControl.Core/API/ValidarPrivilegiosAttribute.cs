using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Security.Claims;

namespace CashControl.Core.API;

/// <summary>
/// Classe de validacao de privilegios.
/// </summary>
public class ValidarPrivilegiosAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Privilegios autorizados na rota.
    /// </summary>
    public string PrivilegiosAutorizados { get; init; } = string.Empty;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.User.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var claimList = identity.Claims.ToList();
        if (IsSystemicUser(claimList))
            return;

        var privilegiosToken = claimList
            .Where(claim => claim.Type == "privilegios")
            .Select(claim => claim.Value.ToUpperInvariant());

        var privilegiosAutorizados = PrivilegiosAutorizados
            .ToUpperInvariant()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (!privilegiosToken.Any(privilegio => privilegiosAutorizados.Any(autorizado => autorizado == privilegio)))
        {
            context.Result = new ObjectResult(ApiErrorResponse.Forbidden("O usuario nao possui privilegio para o endpoint solicitado."))
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }
    }

    private static bool IsSystemicUser(List<Claim> claimList)
    {
        var claim = claimList.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid || c.Type == "groupsid");
        return claim?.Value.Contains("sistemico", StringComparison.OrdinalIgnoreCase) == true;
    }
}
