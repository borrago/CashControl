using CashControl.Core.Application;
using CashControl.Core.CrossCutting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CashControl.Core.API;

[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
public abstract class BaseController : Controller
{
    protected Guid IdUsuario { get; private set; }
    protected string Nome { get; private set; } = "";
    protected string Username { get; private set; } = "";
    public int Tenant { get; private set; }

    private string CamposRetorno { get; set; } = "";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Method.Equals(HttpMethods.Get))
            return;

        // Caso alterar esse nome, alterar em PagedQueryInput.Fields
        CamposRetorno = context.HttpContext.Request.Query
            .Where(w => w.Key.ToLower().Equals("fields"))
            .FirstOrDefault().Value.ToString();
    }

    protected void FillLoggedUser(ILoggedUserProvider loggedUserProvider)
    {
        if (loggedUserProvider.IdUsuario != null) IdUsuario = (Guid)loggedUserProvider.IdUsuario;
        if (!string.IsNullOrWhiteSpace(loggedUserProvider.Nome)) Nome = loggedUserProvider.Nome;
        if (loggedUserProvider != null) Tenant = loggedUserProvider.Tenant;
    }

    protected IActionResult HandleMediatorResult(IMediatorResult mediatorResult, string location = "")
        => mediatorResult.HandleMediatorResult(location, IsSystemicUser(), CamposRetorno);

    private bool IsSystemicUser()
    {
        try
        {
            if (HttpContext?.User?.Identity is not ClaimsIdentity claim || !claim.IsAuthenticated)
                return false;

            var claimGroups = claim.FindFirst(ClaimTypes.GroupSid);

            var claimGroupsValue = claimGroups?.Value;
            if (string.IsNullOrEmpty(claimGroupsValue)) return false;

            return claimGroupsValue.Contains("sistemico");
        }
        catch
        {
            return false;
        }
    }
}
