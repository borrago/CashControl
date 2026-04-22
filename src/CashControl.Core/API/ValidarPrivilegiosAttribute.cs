using CashControl.Core.CrossCutting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Security.Claims;

namespace CashControl.Core.API;

/// <summary>
/// classe de validação de privilégios
/// </summary>
public class ValidarPrivilegiosAttribute : ActionFilterAttribute
{
    public ValidarPrivilegiosAttribute()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }

    /// <summary>
    /// Privilégios autorizados na rota
    /// </summary>
    public string PrivilegiosAutorizados { get; init; } = string.Empty;

    /// <summary>
    /// Validates Level automaticaly
    /// </summary>
    /// <param name="context"></param>
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

        var privilegiosToken = claimList.Where(claim => claim.Type == "privilegios").Select(s => s.Value.ToUpperInvariant());
        var privilegiosAutorizados = PrivilegiosAutorizados.ToUpperInvariant().Split(',').Select(s => s.Trim());

        if (!privilegiosToken.Any(c => privilegiosAutorizados.Any(a => a.Equals(c))))
        {
            context.Result = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(new CustomValidationFailure("Authorization", "O usuário não possui privilégio para o endpoint solicitado."))
            };
        }
    }

    /// <summary>
    /// Verifica se o usuário é sistemico
    /// </summary>
    /// <param name="claimList"></param>
    /// <returns></returns>
    private static bool IsSystemicUser(List<Claim> claimList)
    {
        var claim = claimList.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid || c.Type == "groupsid");
        return claim?.Value.Contains("sistemico", StringComparison.OrdinalIgnoreCase) == true;
    }
}
