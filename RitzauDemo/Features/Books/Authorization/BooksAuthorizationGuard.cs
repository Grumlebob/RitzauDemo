using Microsoft.AspNetCore.Authorization;
using RitzauDemo.Features.Accounts.Authorization;
using RitzauDemo.Features.Accounts.Identity;

namespace RitzauDemo.Features.Books.Authorization;

internal sealed class BooksAuthorizationGuard(
    IAuthorizationService authorization,
    ICurrentUserAccessor currentUser)
{
    public Task RequireReadAsync(CancellationToken cancellationToken = default) =>
        RequirePolicyAsync(AccountAuthorizationPolicies.BooksRead, cancellationToken);

    public Task RequireAdminAsync(CancellationToken cancellationToken = default) =>
        RequirePolicyAsync(AccountAuthorizationPolicies.AdminOnly, cancellationToken);

    private async Task RequirePolicyAsync(string policy, CancellationToken cancellationToken)
    {
        var principal = await currentUser.GetCurrentPrincipalAsync(cancellationToken);
        var result = await authorization.AuthorizeAsync(principal, resource: null, policy);
        if (result.Succeeded)
        {
            return;
        }

        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("An authenticated account is required.");
        }

        throw new BooksAccessDeniedException();
    }
}

internal sealed class BooksAccessDeniedException : UnauthorizedAccessException;
