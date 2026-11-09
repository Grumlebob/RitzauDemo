using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RitzauDemo.Infrastructure.Hosting;
using ApplicationUser = RitzauDemo.Features.Accounts.Identity.ApplicationUser;

namespace RitzauDemo.Features.Accounts.Blazor;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");
        accountGroup.RequireRateLimiting(AppRateLimiting.AccountWritePolicyName);
        accountGroup.MapPost("/Logout", async (
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromForm] string? returnUrl) =>
        {
            await signInManager.SignOutAsync();
            var localReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? "/"
                : $"/{returnUrl.TrimStart('/')}";
            return TypedResults.LocalRedirect($"~{localReturnUrl}");
        })
        .RequireAuthorization();

        return accountGroup;
    }
}
