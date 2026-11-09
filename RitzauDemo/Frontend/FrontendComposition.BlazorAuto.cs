using RitzauDemo.Client.Features.Books.AuthorBookcase;
using RitzauDemo.Client.Features.Books.UserBookcase;
using RitzauDemo.Components;
using RitzauDemo.Features.Accounts.Blazor;
using RitzauDemo.Infrastructure.Hosting;
using ClientImports = RitzauDemo.Client._Imports;

namespace RitzauDemo.Frontend;

internal static class FrontendComposition
{
    public static void AddFrontendServices(IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization(options =>
                options.SerializationCallback = AccountAuthenticationStateSerialization.Serialize);

        services.AddBlazorIdentityUi();
        services.AddScoped<AuthorBookcaseState>();
        services.AddScoped<UserBookcaseState>();
    }

    public static void UseFrontendMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseWhen(ShouldRenderStatusCodePage, branch =>
        {
            branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        });
    }

    public static void UseFrontendStaticFiles(WebApplication app)
    {
    }

    public static void MapFrontendEndpoints(WebApplication app)
    {
        app.MapStaticAssets();
        app.MapPublicPageHeadRequests();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(ClientImports).Assembly);
        app.MapAdditionalIdentityEndpoints();
    }

    private static bool ShouldRenderStatusCodePage(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            return false;
        }

        return HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
    }
}
