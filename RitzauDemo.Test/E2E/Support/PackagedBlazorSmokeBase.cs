using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace RitzauDemo.Test.E2E.Support;

public abstract class PackagedBlazorSmokeBase : PageTest
{
    private static readonly Uri BaseUri = new(GetBaseUrl());

    public override Task<BrowserTypeLaunchOptions?> LaunchOptionsAsync() =>
        Task.FromResult<BrowserTypeLaunchOptions?>(new BrowserTypeLaunchOptions
        {
            Headless = string.Equals(Environment.GetEnvironmentVariable("E2E_HEADLESS"), "1", StringComparison.OrdinalIgnoreCase)
        });

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        IgnoreHTTPSErrors = true,
        ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
    };

    protected Task<IResponse?> GoToAsync(string path)
    {
        var target = new Uri(BaseUri, path.TrimStart('/'));
        return Page.GotoAsync(target.ToString(), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
    }

    protected async Task GoHomeAndWaitForInteractivityAsync()
    {
        await GoToAsync("/");
        await WaitForInteractivityAsync();
    }

    protected async Task WaitForInteractivityAsync()
    {
        await Expect(Page.GetByTestId("app-interactivity-probe"))
            .ToHaveAttributeAsync("data-interactive", "true", new LocatorAssertionsToHaveAttributeOptions { Timeout = 45_000 });
    }

    protected async Task ReloadAndWaitForInteractivityAsync()
    {
        await Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForInteractivityAsync();
    }

    protected async Task RunWithFailureScreenshotAsync(Func<Task> test)
    {
        var artifactDirectory = E2EArtifactPaths.PlaywrightPath();
        Directory.CreateDirectory(artifactDirectory);
        Page.SetDefaultNavigationTimeout(90_000);
        Page.SetDefaultTimeout(45_000);
        await Context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true, Sources = true });

        try
        {
            await test();
            await Context.Tracing.StopAsync();
        }
        catch
        {
            var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");
            try
            {
                await Context.Tracing.StopAsync(new TracingStopOptions
                {
                    Path = Path.Combine(artifactDirectory, $"{GetType().Name}-{stamp}.zip")
                });
            }
            catch (Exception traceException)
            {
                Console.Error.WriteLine($"Browser trace capture failed: {traceException.Message}");
            }

            try
            {
                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = Path.Combine(artifactDirectory, $"{GetType().Name}-{stamp}.png"),
                    FullPage = true,
                    Timeout = 10_000
                });
            }
            catch (Exception screenshotException)
            {
                Console.Error.WriteLine($"Browser failure screenshot failed: {screenshotException.Message}");
            }

            throw;
        }
    }

    private static string GetBaseUrl()
    {
        var baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL");
        return string.IsNullOrWhiteSpace(baseUrl) ? "https://localhost:7186/" : baseUrl.TrimEnd('/') + "/";
    }
}
