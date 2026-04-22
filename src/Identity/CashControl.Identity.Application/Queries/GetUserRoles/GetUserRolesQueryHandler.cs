using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Queries.GetUserRoles;

public class GetUserRolesQueryHandler(IIdentityService identityService) : IQueryHandler<GetUserRolesQueryInput, GetUserRolesQueryResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<GetUserRolesQueryResult> Handle(GetUserRolesQueryInput query, CancellationToken cancellationToken)
    {
        var roles = await _identityService.GetRolesAsync(query.UserId, cancellationToken);

        return (GetUserRolesQueryResult)new GetUserRolesQueryResult
        {
            UserId = query.UserId,
            Roles = roles
        }.WithHttpStatusCode(HttpStatusCode.OK);
    }
}
