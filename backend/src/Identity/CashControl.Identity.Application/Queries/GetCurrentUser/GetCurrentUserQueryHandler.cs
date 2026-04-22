using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(IIdentityService identityService) : IQueryHandler<GetCurrentUserQueryInput, GetCurrentUserQueryResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<GetCurrentUserQueryResult> Handle(GetCurrentUserQueryInput query, CancellationToken cancellationToken)
    {
        var result = await _identityService.GetCurrentUserAsync(query.Id, cancellationToken);

        if (result is null)
            return new GetCurrentUserQueryResult().WithHttpStatusCode(HttpStatusCode.NotFound) as GetCurrentUserQueryResult ?? throw new Core.Application.ApplicationException("Falha ao buscar informação.");

        return (GetCurrentUserQueryResult)new GetCurrentUserQueryResult
        {
            Id = result.Id,
            Email = result.Email,
            FullName = result.FullName,
            PhoneNumber = result.PhoneNumber,
            UserName = result.UserName,
            Roles = result.Roles,
        }.WithHttpStatusCode(HttpStatusCode.OK);
    }
}
