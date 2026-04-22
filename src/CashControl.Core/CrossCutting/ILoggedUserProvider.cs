using Microsoft.AspNetCore.Http;

namespace CashControl.Core.CrossCutting;

public interface ILoggedUserProvider
{
    Guid? IdUsuario { get; }

    string Nome { get; }

    int Tenant { get; }
    bool IsLogged();

    void SetAuthorizationFromContext(HttpContext httpContext);
}