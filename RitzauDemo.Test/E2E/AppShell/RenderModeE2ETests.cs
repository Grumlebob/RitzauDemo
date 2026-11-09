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
            await GoHomeAndWaitForInteractivityAsync();

            var assignedMode = (await Page.GetByTestId("assigned-render-mode").InnerTextAsync()).Trim();
            Assert.True(
                assignedMode is "Interactive Server" or "Interactive WebAssembly" or "Interactive Auto",
                $"Expected assigned render mode to be interactive, but was '{assignedMode}'.");

            var currentRenderer = (await Page.GetByTestId("current-renderer").InnerTextAsync()).Trim();
            Assert.True(
                currentRenderer is "Server" or "WebAssembly",
                $"Expected hydrated renderer to be Server or WebAssembly, but was '{currentRenderer}'.");
        });
    }
}
