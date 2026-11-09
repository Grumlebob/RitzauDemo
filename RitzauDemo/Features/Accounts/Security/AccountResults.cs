using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using RitzauDemo.Core.Features.Accounts.Contracts;

namespace RitzauDemo.Features.Accounts.Security;

internal static class AccountResults
{
    public static IResult Success(string message) => Results.Ok(new AccountMessage(message));
    public static IResult InvalidToken() =>
        AccountProblem.Create(400, "TokenInvalid", "The operation could not be completed", "The request is invalid or unavailable.");
    public static IResult Denied() =>
        AccountProblem.Create(401, "CredentialsInvalid", "Sign-in failed", "Check your credentials and account confirmation, then try again.");
    public static IResult SessionRequired() =>
        AccountProblem.Create(401, "AuthenticationRequired", "Sign-in required");
    public static IResult DeliveryUnavailable() =>
        AccountProblem.Create(503, "AccountServiceUnavailable", "Account requests are temporarily unavailable", "Please try again later.");
    public static IResult Conflict() =>
        AccountProblem.Create(409, "AccountConflict", "The account could not be updated", "The request conflicts with the current account state.");
    public static IResult IdentityFailure(IdentityResult result, string? passwordField = null)
    {
        var codes = result.Errors.Select(error => error.Code).ToArray();
        if (codes.Any(code => code is "DuplicateEmail" or "DuplicateUserName"))
        {
            return Conflict();
        }
        if (codes.Any(code => code.StartsWith("Password", StringComparison.Ordinal)) || passwordField is not null)
        {
            return AccountProblem.Validation(400, "PasswordRejected", "Password rejected", "Choose a password that meets the account password policy.",
                new Dictionary<string, string[]> { [passwordField ?? "password"] = ["The password does not meet the account password policy."] });
        }
        return AccountProblem.Validation(400, "RequestInvalid", "Account request failed", "The request could not be completed.",
            new Dictionary<string, string[]> { ["account"] = ["The request could not be completed."] });
    }

    public static bool IsPersistenceFailure(Exception exception) =>
        exception is DbException or DbUpdateException or TimeoutException or RetryLimitExceededException;
}
