using Microsoft.AspNetCore.Components.Authorization;
using RitzauDemo.Features.Accounts.Identity;

namespace RitzauDemo.Features.Accounts.Blazor;

internal static class BlazorIdentityServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorIdentityUi(this IServiceCollection services)
    {
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<ICurrentUserAccessor, BlazorCurrentUserAccessor>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        return services;
    }
}
