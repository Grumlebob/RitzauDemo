using System.Security.Claims;

namespace RitzauDemo.Features.Accounts.Identity;

internal interface ICurrentUserAccessor
{
    ValueTask<ClaimsPrincipal> GetCurrentPrincipalAsync(CancellationToken cancellationToken = default);

    ValueTask<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default);

    ValueTask<string> GetRequiredUserIdAsync(CancellationToken cancellationToken = default);
}
