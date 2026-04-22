using CashControl.Core.CrossCutting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
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
        var token = context.HttpContext.Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        var claimList = ExtractClaims(token);
        if (IsSystemicUser(claimList))
            return;

        var privilegiosToken = claimList.Where(claim => claim.Type == "privilegios").Select(s => s.Value.ToUpper());

        var PrivilegiosAutorizadosArray = PrivilegiosAutorizados.ToUpper().Split(',').Select(s => s.Trim());

        if (!privilegiosToken.Any(c => PrivilegiosAutorizadosArray.Any(a => a.Equals(c))))
            context.Result = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Content = JsonConvert.SerializeObject(new CustomValidationFailure(HeaderNames.Authorization, "O usuário não possui privilégio para o endpoint solicitado."))
            };
    }

    /// <summary>
    /// Verifica se o usuário é sistemico
    /// </summary>
    /// <param name="claimList"></param>
    /// <returns></returns>
    private static bool IsSystemicUser(List<Claim> claimList)
    {
        var claim = claimList.FirstOrDefault(claim => claim.Type == "groupsid");
        if (claim == null)
            return false;

        return claim.Value.Contains("sistemico");
    }

    /// <summary>
    /// método de estrair claims do token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static List<Claim> ExtractClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenS = handler.ReadJwtToken(token);

        return [.. tokenS.Claims];
    }
}
