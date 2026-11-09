using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RitzauDemo.Core.Features.Accounts.Contracts;
using RitzauDemo.Features.Accounts.Security;
using static RitzauDemo.Features.Accounts.Security.AccountResults;
using ApplicationUser = RitzauDemo.Features.Accounts.Identity.ApplicationUser;

namespace RitzauDemo.Features.Accounts.Session;

internal sealed class AccountRevocationHandler(
    UserManager<ApplicationUser> users,
    AccountMutationTransactions transactions,
    AccountSessionOperations sessions)
{
    public async Task<IResult> LogoutAllAsync(AccountPasswordRequest request, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(principal);
        if (string.IsNullOrEmpty(userId)) return Denied();
        var (response, committed) = await transactions.ExecuteAsync(userId, async (attempt, user) =>
        {
            var passwordCheck = await attempt.SignIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!passwordCheck.Succeeded) return (Denied(), true);
            var result = await attempt.Users.UpdateSecurityStampAsync(user);
            if (!result.Succeeded) return (IdentityFailure(result), false);
            return (Success("All sessions were signed out."), true);
        }, cancellationToken, principal: principal);
        return committed && response is IStatusCodeHttpResult { StatusCode: StatusCodes.Status200OK }
            ? await sessions.LogoutAsync()
            : response;
    }
}
