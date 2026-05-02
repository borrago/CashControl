using CashControl.Core.Application;
using CashControl.Identity.Application.Services;
using System.Net;

namespace CashControl.Identity.Application.Queries.GetUserById;

public class GetUserByIdQueryHandler(IIdentityService identityService) : IQueryHandler<GetUserByIdQueryInput, GetUserByIdQueryResult>
{
    private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

    public async Task<GetUserByIdQueryResult> Handle(GetUserByIdQueryInput query, CancellationToken cancellationToken)
    {
        var result = await _identityService.GetByIdAsync(query.UserId, cancellationToken);

        if (result is null)
            return (GetUserByIdQueryResult)new GetUserByIdQueryResult().WithHttpStatusCode(HttpStatusCode.NotFound);

        return (GetUserByIdQueryResult)new GetUserByIdQueryResult
        {
            Id = result.Id,
            Email = result.Email,
            FullName = result.FullName,
            PhoneNumber = result.PhoneNumber,
            Tenant = result.Tenant,
            CanImpersonateUsers = result.CanImpersonateUsers,
            IsImpersonating = result.IsImpersonating,
            CanStopImpersonation = result.CanStopImpersonation,
            ImpersonatedByEmail = result.ImpersonatedByEmail,
            UserName = result.UserName,
            Roles = result.Roles,
        }.WithHttpStatusCode(HttpStatusCode.OK);
    }
}
