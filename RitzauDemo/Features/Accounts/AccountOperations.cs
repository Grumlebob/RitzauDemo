using RitzauDemo.Core.Features.Accounts.Contracts;
using RitzauDemo.Features.Accounts.Session;

namespace RitzauDemo.Features.Accounts;

internal sealed class AccountOperations(
    IHttpContextAccessor accessor,
    AccountRevocationHandler revocation)
{
    private HttpContext Context => accessor.HttpContext ?? throw new InvalidOperationException("An account request requires an HTTP context.");

    public Task<IResult> LogoutAllAsync(AccountPasswordRequest request) => revocation.LogoutAllAsync(request, Context.User, Context.RequestAborted);
}
