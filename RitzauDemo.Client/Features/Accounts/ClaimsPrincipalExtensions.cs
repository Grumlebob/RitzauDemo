using System.Security.Claims;

namespace RitzauDemo.Client.Features.Accounts;

internal static class ClaimsPrincipalExtensions
{
    public static string? GetStableUserId(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
    }
}
