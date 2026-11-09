using Microsoft.Extensions.Diagnostics.HealthChecks;
using RitzauDemo.Features.Accounts;
using RitzauDemo.Features.Accounts.Endpoints;
using RitzauDemo.Features.Accounts.Identity;
using RitzauDemo.Features.Accounts.Security;
using RitzauDemo.Features.Books;
using RitzauDemo.Features.Books.AuthorBookcase.Seed;
using RitzauDemo.Features.Books.UserBookcase.Seed;
using RitzauDemo.Frontend;
using RitzauDemo.Frontend.OpenApi;
using RitzauDemo.Infrastructure.Hosting;

if (OpenApiMetadataHosting.IsOpenApiGenerationProcess())
{
    var metadataBuilder = OpenApiMetadataHosting.CreateBuilder();
    var metadataApp = metadataBuilder.Build();
    OpenApiMetadataHosting.MapEndpoints(metadataApp);
    await metadataApp.RunAsync();
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.AddAppObservability();
builder.Services.AddAppOptions(builder.Configuration);
builder.Services.AddAccountProblemDetails();
builder.Services.AddAppApiJson();
builder.Services.AddValidation();

var healthChecks = builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

builder.Services.AddAppForwarding(builder.Configuration);
builder.Services.AddAppAntiforgery(builder.Environment);
builder.Services.AddAppCachingAndDataProtection(builder.Configuration, builder.Environment, healthChecks);
builder.Services.AddAppPersistence(builder.Configuration, healthChecks);
builder.Services.AddAppRateLimiting(builder.Configuration);
builder.Services.AddBooksFeature(builder.Configuration);
builder.Services.AddIdentityBackend();
builder.Services.AddAccountFeature(builder.Configuration, builder.Environment);
FrontendComposition.AddFrontendServices(builder.Services);

var app = builder.Build();

app.UseAppRequestLogging();
app.UseForwardedHeaders();

FrontendComposition.UseFrontendMiddleware(app);

// Account middleware reads endpoint markers before authentication and body binding.
app.UseRouting();
app.UseHttpsRedirection();
app.UseAccountRequestPolicies();

app.UseAuthentication();
app.UseAppRateLimiting();
app.UseAccountJsonBodyLimits();
app.UseAuthorization();
app.UseAccountPrincipalGuards();

app.UseAntiforgery();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.TryAdd("Permissions-Policy", "local-network-access=()");
    await next();
});

FrontendComposition.UseFrontendStaticFiles(app);

await app.ApplyAppMigrationsAsync();
await app.SeedConfiguredAccountsAsync();
await app.SeedDemoUserBooksAsync();
await app.SeedAuthorBooksAsync();

FrontendComposition.MapFrontendEndpoints(app);
app.MapAppHealthChecks();
app.MapBooksFeature();
app.MapClientConfiguration();
app.MapAccountEndpoints();

app.Run();
