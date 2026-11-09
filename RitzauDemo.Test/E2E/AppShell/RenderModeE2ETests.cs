using RitzauDemo.Test.E2E.Support;
using RitzauDemo.Test.TestSupport.Integration;

namespace RitzauDemo.Test.E2E.AppShell;

[Collection(TestCollectionNames.E2E)]
public sealed class RenderModeE2ETests : PackagedBlazorSmokeBase
{
    [Fact(Skip = "Set RUN_E2E=1 to run the packaged Blazor render smoke.", SkipUnless = nameof(E2ETestGuard.IsEnabled), SkipType = typeof(E2ETestGuard))]
    [Trait("Category", "E2E")]
    public async Task HomePage_ReportsAssignedAutoAndHydratedRenderer()
    {
        await RunWithFailureScreenshotAsync(async () =>
        {
            var response = await GoToAsync("/");
            Assert.NotNull(response);
            var prerenderedHtml = await response!.TextAsync();
            Assert.Contains("data-testid=\"public-catalog-panel\"", prerenderedHtml, StringComparison.Ordinal);
            Assert.Contains("data-interactive=\"false\"", prerenderedHtml, StringComparison.Ordinal);

            await GoHomeAndWaitForInteractivityAsync();

            var configuredMode = (await Page.GetByTestId("configured-render-mode").InnerTextAsync()).Trim();
            Assert.Equal("Interactive Auto", configuredMode);

            var currentRenderer = (await Page.GetByTestId("current-renderer").InnerTextAsync()).Trim();
            Assert.Equal("Server", currentRenderer);

            await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
            await ReloadAndWaitForInteractivityAsync();

            var laterRenderer = (await Page.GetByTestId("current-renderer").InnerTextAsync()).Trim();
            Assert.Equal("WebAssembly", laterRenderer);
        });
    }
}
