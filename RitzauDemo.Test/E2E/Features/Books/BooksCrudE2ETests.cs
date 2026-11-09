using Microsoft.Playwright;
using RitzauDemo.Test.E2E.Support;
using RitzauDemo.Test.TestSupport.Integration;

namespace RitzauDemo.Test.E2E.Features.Books;

[Collection(TestCollectionNames.E2E)]
public sealed class BooksCrudE2ETests : PackagedBlazorSmokeBase
{
    [Fact(Skip = "Set RUN_E2E=1 to run packaged browser acceptance tests.", SkipUnless = nameof(E2ETestGuard.IsEnabled), SkipType = typeof(E2ETestGuard))]
    [Trait("Category", "E2E")]
    public async Task AnonymousCanBrowsePublicCatalogWithoutMutationControls()
    {
        await RunWithFailureScreenshotAsync(async () =>
        {
            var response = await GoToAsync("/");
            Assert.NotNull(response);
            var prerenderedHtml = await response!.TextAsync();
            Assert.Contains("data-testid=\"public-catalog-panel\"", prerenderedHtml, StringComparison.Ordinal);
            Assert.Contains("data-testid=\"author-book-list\"", prerenderedHtml, StringComparison.Ordinal);

            await GoHomeAndWaitForInteractivityAsync();
            await Expect(Page.GetByTestId("public-catalog-panel")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("private-books-sign-in")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("user-books-panel")).ToHaveCountAsync(0);

            var firstPublicBook = Page.GetByTestId("author-book-list").GetByRole(AriaRole.Link).First;
            await firstPublicBook.ClickAsync();
            await Expect(Page.GetByTestId("book-details")).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Edit book", Exact = true }))
                .ToHaveCountAsync(0);
            await Expect(Page.GetByTestId("book-delete-request")).ToHaveCountAsync(0);
        });
    }

    [Fact(Skip = "Set RUN_E2E=1 to run packaged browser acceptance tests.", SkipUnless = nameof(E2ETestGuard.IsEnabled), SkipType = typeof(E2ETestGuard))]
    [Trait("Category", "E2E")]
    public async Task UserCanReadOwnBooksButCannotSeeOrOpenWriteRoutes()
    {
        await RunWithFailureScreenshotAsync(async () =>
        {
            await SignInAsync(SeededE2EAccounts.ResolveUser());
            await GoHomeAndWaitForInteractivityAsync();

            await Expect(Page.GetByTestId("user-book-list")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("add-book")).ToHaveCountAsync(0);
            var firstUserBook = Page.GetByTestId("user-book-list").Locator("article").First;
            await Expect(firstUserBook).ToBeVisibleAsync();
            var detailsLink = firstUserBook.GetByRole(AriaRole.Link).First;
            var detailsHref = await detailsLink.GetAttributeAsync("href");
            Assert.False(string.IsNullOrWhiteSpace(detailsHref));
            var bookId = int.Parse(detailsHref!.Trim('/').Split('/')[^1], System.Globalization.CultureInfo.InvariantCulture);

            await detailsLink.ClickAsync();
            await Expect(Page.GetByTestId("book-details")).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Edit book", Exact = true }))
                .ToHaveCountAsync(0);
            await Expect(Page.GetByTestId("book-delete-request")).ToHaveCountAsync(0);

            await GoToAsync($"/books/{bookId}/edit");
            await Expect(Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Access denied", Exact = true }))
                .ToBeVisibleAsync();
            Assert.Equal("/Account/AccessDenied", new Uri(Page.Url).AbsolutePath);
            await Expect(Page.GetByTestId("book-form")).ToHaveCountAsync(0);

            await GoToAsync("/books/new");
            await Expect(Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Access denied", Exact = true }))
                .ToBeVisibleAsync();
            Assert.Equal("/Account/AccessDenied", new Uri(Page.Url).AbsolutePath);
            await Expect(Page.GetByTestId("book-form")).ToHaveCountAsync(0);
        });
    }

    [Fact(Skip = "Set RUN_E2E=1 to run packaged browser acceptance tests.", SkipUnless = nameof(E2ETestGuard.IsEnabled), SkipType = typeof(E2ETestGuard))]
    [Trait("Category", "E2E")]
    public async Task UserBookListShowsSafeErrorWhenBooksApiFails()
    {
        await RunWithFailureScreenshotAsync(async () =>
        {
            await GoHomeAndWaitForInteractivityAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await ReloadAndWaitForInteractivityAsync();
            Assert.Equal("WebAssembly", (await Page.GetByTestId("current-renderer").InnerTextAsync()).Trim());

            await Page.RouteAsync("**/api/books", route => route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 503,
                ContentType = "application/problem+json",
                Body = "{\"title\":\"Unavailable\"}"
            }));

            await SignInAsync(SeededE2EAccounts.ResolveUser());
            await GoHomeAndWaitForInteractivityAsync();
            await Expect(Page.GetByRole(AriaRole.Alert).Filter(new LocatorFilterOptions { HasText = "Your books could not be loaded." }))
                .ToBeVisibleAsync();
        });
    }

    [Fact(Skip = "Set RUN_E2E=1 to run packaged browser acceptance tests.", SkipUnless = nameof(E2ETestGuard.IsEnabled), SkipType = typeof(E2ETestGuard))]
    [Trait("Category", "E2E")]
    public async Task AdminCanCreateValidateReloadUpdateDeleteAndLogout()
    {
        await RunWithFailureScreenshotAsync(async () =>
        {
            await SignInAsync(SeededE2EAccounts.ResolveAdmin());
            await GoHomeAndWaitForInteractivityAsync();
            await Expect(Page.GetByTestId("user-book-empty-state")).ToBeVisibleAsync();

            var title = $"Interview UI book {Guid.NewGuid():N}";
            var updatedTitle = $"{title} updated";
            await Page.GetByTestId("add-book").ClickAsync();
            await Expect(Page.GetByTestId("book-form")).ToBeVisibleAsync();

            await Page.GetByTestId("book-save").ClickAsync();
            await Expect(Page.GetByTestId("book-form").GetByText("The Title field is required.").First)
                .ToBeVisibleAsync();

            await Page.GetByTestId("book-title").FillAsync(title);
            await Page.GetByTestId("book-author").FillAsync("Browser acceptance author");
            await Page.GetByTestId("book-url").FillAsync("javascript:alert(1)");
            await Page.GetByTestId("book-save").ClickAsync();
            await Expect(Page.GetByTestId("book-form").GetByText("URL must be an absolute HTTP or HTTPS URL.").First)
                .ToBeVisibleAsync();

            await Page.GetByTestId("book-url").FillAsync("https://example.test/interview-ui-book");
            await Page.GetByTestId("book-save").ClickAsync();
            await Expect(Page.GetByTestId("book-details")).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = title, Exact = true }))
                .ToBeVisibleAsync();

            var bookId = int.Parse(new Uri(Page.Url).AbsolutePath.Trim('/').Split('/')[^1], System.Globalization.CultureInfo.InvariantCulture);
            await ReloadAndWaitForInteractivityAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = title, Exact = true }))
                .ToBeVisibleAsync();

            await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Edit book", Exact = true }).ClickAsync();
            await Expect(Page.GetByTestId("book-edit-page")).ToBeVisibleAsync();
            await Page.GetByTestId("book-title").FillAsync(updatedTitle);
            await Page.GetByTestId("book-save").ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = updatedTitle, Exact = true }))
                .ToBeVisibleAsync();

            await Page.GetByTestId("book-delete-request").ClickAsync();
            await Expect(Page.GetByTestId("book-delete-confirmation")).ToBeVisibleAsync();
            await Page.GetByTestId("book-delete-confirm").ClickAsync();
            await Expect(Page.GetByTestId("user-books-panel")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId($"book-row-{bookId}")).ToHaveCountAsync(0);

            await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Logout", Exact = true }).ClickAsync();
            await GoHomeAndWaitForInteractivityAsync();
            await Expect(Page.GetByTestId("private-books-sign-in")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("user-books-panel")).ToHaveCountAsync(0);
        });
    }

    private async Task SignInAsync(SeededE2EAccount account)
    {
        await GoToAsync("/Account/Login");
        await Page.Locator("#Input\\.Email").FillAsync(account.Email);
        await Page.Locator("#Input\\.Password").FillAsync(account.Password);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log in", Exact = true }).ClickAsync();
        await WaitForInteractivityAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Logout", Exact = true }))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 30_000 });
    }
}
