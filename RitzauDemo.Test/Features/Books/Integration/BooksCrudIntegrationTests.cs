using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RitzauDemo.Core.Features.Accounts.Contracts;
using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Core.Features.Books.Domain;
using RitzauDemo.Core.Features.Books.UseCases.CreateBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBooks;
using RitzauDemo.Core.Features.Books.UseCases.UpdateBook;
using RitzauDemo.Features.Books.Authorization;
using RitzauDemo.Infrastructure.Persistence;
using RitzauDemo.Test.TestSupport.Integration;
using StackExchange.Redis;

namespace RitzauDemo.Test.Features.Books.Integration;

[Collection(TestCollectionNames.Integration)]
public sealed class BooksCrudIntegrationTests : IAsyncLifetime
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _adminClient;
    private readonly HttpClient _userClient;
    private readonly HttpClient _anonymousClient;
    private readonly Func<Task> _resetDatabase;

    public BooksCrudIntegrationTests(WebAppFactory factory)
    {
        _factory = factory;
        _adminClient = factory.CreateAuthenticatedClient("crud-admin@example.test", AccountRoleNames.Admin);
        _userClient = factory.CreateAuthenticatedClient("crud-user@example.test", AccountRoleNames.User);
        _anonymousClient = factory.HttpClient;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    [Fact]
    public async Task AdminCanCreateListReadUpdateAndDeleteOwnBook()
    {
        var create = new CreateBookRequest
        {
            Title = "CRUD demo book",
            Author = "Demo author",
            Url = "https://example.test/books/crud-demo"
        };

        using var createResponse = await _adminClient.PostAsJsonAsync("/api/books", create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateBookResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);

        using var listResponse = await _adminClient.GetAsync("/api/books");
        listResponse.EnsureSuccessStatusCode();
        var listed = await listResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(listed);
        var listedBook = Assert.Single(listed!.Books, book => book.Id == created.Id);
        Assert.Equal(create.Title, listedBook.Title);

        using var otherUserResponse = await _userClient.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, otherUserResponse.StatusCode);

        using var warmedDetailResponse = await _adminClient.GetAsync($"/api/books/{created.Id}");
        warmedDetailResponse.EnsureSuccessStatusCode();
        var warmedDetail = await warmedDetailResponse.Content.ReadFromJsonAsync<GetBookResponse>();
        Assert.NotNull(warmedDetail);
        Assert.Equal(create.Title, warmedDetail!.Title);

        var update = new UpdateBookRequest
        {
            Id = created.Id,
            Title = "CRUD demo book updated",
            Author = create.Author,
            Url = create.Url
        };
        using var updateResponse = await _adminClient.PutAsJsonAsync($"/api/books/{created.Id}", update);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        using var readResponse = await _adminClient.GetAsync($"/api/books/{created.Id}");
        readResponse.EnsureSuccessStatusCode();
        var read = await readResponse.Content.ReadFromJsonAsync<GetBookResponse>();
        Assert.NotNull(read);
        Assert.Equal(update.Title, read!.Title);

        using var refreshedListResponse = await _adminClient.GetAsync("/api/books");
        refreshedListResponse.EnsureSuccessStatusCode();
        var refreshedList = await refreshedListResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(refreshedList);
        Assert.Equal(update.Title, Assert.Single(refreshedList!.Books).Title);

        using var deleteResponse = await _adminClient.DeleteAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var missingResponse = await _adminClient.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);

        using var finalListResponse = await _adminClient.GetAsync("/api/books");
        finalListResponse.EnsureSuccessStatusCode();
        var finalList = await finalListResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(finalList);
        Assert.Empty(finalList!.Books);
    }

    [Fact]
    public async Task UserCanReadOwnBooksButCannotWriteAndAdminCannotCrossOwnership()
    {
        await EnsureAuthenticatedAccountsAsync();
        var userBookId = await AddBookAsync("User-owned sample", "crud-user@example.test");
        var adminBookId = await AddBookAsync("Admin-owned sample", "crud-admin@example.test");

        using var userListResponse = await _userClient.GetAsync("/api/books");
        userListResponse.EnsureSuccessStatusCode();
        var userList = await userListResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(userList);
        Assert.Collection(userList!.Books, book => Assert.Equal(userBookId, book.Id));

        using var userReadResponse = await _userClient.GetAsync($"/api/books/{userBookId}");
        Assert.Equal(HttpStatusCode.OK, userReadResponse.StatusCode);

        var userBookPage = await _userClient.GetStringAsync($"/books?bookId={userBookId}&bookMode=view");
        Assert.Contains("data-testid=\"book-page-view\"", userBookPage, StringComparison.Ordinal);
        Assert.DoesNotContain("data-testid=\"book-edit-pencil\"", userBookPage, StringComparison.Ordinal);
        Assert.DoesNotContain("data-testid=\"book-delete\"", userBookPage, StringComparison.Ordinal);

        var userEditPage = await _userClient.GetStringAsync($"/books?bookId={userBookId}&bookMode=edit");
        Assert.Contains("Administrator access required", userEditPage, StringComparison.Ordinal);
        Assert.DoesNotContain("data-testid=\"book-save\"", userEditPage, StringComparison.Ordinal);

        using var hiddenReadResponse = await _userClient.GetAsync($"/api/books/{adminBookId}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenReadResponse.StatusCode);

        using var createResponse = await _userClient.PostAsJsonAsync("/api/books", new CreateBookRequest
        {
            Title = "Denied user write",
            Author = "Reader"
        });
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);

        using var updateResponse = await _userClient.PutAsJsonAsync($"/api/books/{userBookId}", new UpdateBookRequest
        {
            Id = userBookId,
            Title = "Denied user update",
            Author = "Reader"
        });
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);

        using var deleteResponse = await _userClient.DeleteAsync($"/api/books/{userBookId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

        using var crossOwnerUpdate = await _adminClient.PutAsJsonAsync($"/api/books/{userBookId}", new UpdateBookRequest
        {
            Id = userBookId,
            Title = "Denied cross-owner update",
            Author = "Admin"
        });
        Assert.Equal(HttpStatusCode.NotFound, crossOwnerUpdate.StatusCode);

        using var crossOwnerDelete = await _adminClient.DeleteAsync($"/api/books/{userBookId}");
        Assert.Equal(HttpStatusCode.NotFound, crossOwnerDelete.StatusCode);

        await using var db = await CreateDbContextAsync();
        Assert.Equal("User-owned sample", await db.Books.Where(book => book.Id == userBookId).Select(book => book.Title).SingleAsync());
        Assert.Equal("Admin-owned sample", await db.Books.Where(book => book.Id == adminBookId).Select(book => book.Title).SingleAsync());
        Assert.Equal(2, await db.UserBooks.CountAsync());
    }

    [Fact]
    public async Task AnonymousCannotReadPrivateBooks()
    {
        using var response = await _anonymousClient.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DirectServerApiEnforcesReadAndWritePolicies()
    {
        await EnsureAuthenticatedAccountsAsync();
        using var directUserClient = _factory.CreateAuthenticatedClient("direct-user@example.test", AccountRoleNames.User);
        using var directAdminClient = _factory.CreateAuthenticatedClient("direct-admin@example.test", AccountRoleNames.Admin);
        using var directUserSession = await directUserClient.GetAsync("/api/auth/session");
        directUserSession.EnsureSuccessStatusCode();
        using var directAdminSession = await directAdminClient.GetAsync("/api/auth/session");
        directAdminSession.EnsureSuccessStatusCode();

        await using var scope = _factory.Services.CreateAsyncScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var api = scope.ServiceProvider.GetRequiredService<IBooksApi>();
        httpContextAccessor.HttpContext = CreateBookContext("direct-user@example.test", AccountRoleNames.User);

        _ = await api.GetAsync();
        await Assert.ThrowsAsync<BooksAccessDeniedException>(() => api.CreateAsync(new CreateBookRequest
        {
            Title = "Direct user write",
            Author = "Reader"
        }));

        httpContextAccessor.HttpContext = CreateBookContext("direct-admin@example.test", AccountRoleNames.Admin);
        var created = await api.CreateAsync(new CreateBookRequest
        {
            Title = "Direct admin write",
            Author = "Administrator"
        });
        var adminBooks = await api.GetAsync();
        var directBook = Assert.Single(adminBooks.Books);
        Assert.Equal(created.Id, directBook.Id);
        Assert.Equal("Direct admin write", directBook.Title);
    }

    [Fact]
    public async Task RedisInvalidatesAnotherNodesWarmedBooksList()
    {
        await EnsureAuthenticatedAccountsAsync();

        await using var secondNode = new WebAppFactory(new WebAppFactoryOptions
        {
            PostgresConnectionString = _factory.ConnectionString,
            RedisConnectionString = _factory.RedisConnectionString,
            CacheInvalidationEnabled = true,
            CacheInvalidationNodeId = $"integration-node-two-{Guid.NewGuid():N}",
            CacheInvalidationChannelName = _factory.CacheInvalidationChannelName,
            RunMigrationsAtStartup = false,
            InitializeDatabaseRespawner = false,
            LocalListTtlSeconds = 60,
            LocalItemTtlSeconds = 60,
            UseProcessEnvironmentOverrides = false
        });
        await secondNode.InitializeAsync();

        using var redis = await ConnectionMultiplexer.ConnectAsync(_factory.RedisConnectionString);
        var redisServer = redis.GetServer(redis.GetEndPoints().Single());
        await WaitForRedisSubscribersAsync(redisServer, _factory.CacheInvalidationChannelName, expectedCount: 2);

        using var warmedListResponse = await _adminClient.GetAsync("/api/books");
        warmedListResponse.EnsureSuccessStatusCode();
        var warmedList = await warmedListResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(warmedList);
        Assert.Empty(warmedList!.Books);

        using var writer = secondNode.CreateAuthenticatedClient("crud-admin@example.test", AccountRoleNames.Admin);
        using var createResponse = await writer.PostAsJsonAsync("/api/books", new CreateBookRequest
        {
            Title = "Cross-node cache refresh",
            Author = "Cache writer"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var stopWatch = Stopwatch.StartNew();
        while (stopWatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            using var listResponse = await _adminClient.GetAsync("/api/books");
            listResponse.EnsureSuccessStatusCode();
            var list = await listResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
            if (list?.Books is [{ Title: "Cross-node cache refresh" }])
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        Assert.Fail("The first node did not observe the second node's cache invalidation.");
    }

    public async ValueTask InitializeAsync() => await _resetDatabase();

    public async ValueTask DisposeAsync() => await _resetDatabase();

    private async Task EnsureAuthenticatedAccountsAsync()
    {
        using var adminResponse = await _adminClient.GetAsync("/api/auth/session");
        adminResponse.EnsureSuccessStatusCode();
        using var userResponse = await _userClient.GetAsync("/api/auth/session");
        userResponse.EnsureSuccessStatusCode();
    }

    private async Task<int> AddBookAsync(string title, string ownerUserId)
    {
        await using var db = await CreateDbContextAsync();
        var book = new Book { Title = title };
        db.Books.Add(book);
        db.UserBooks.Add(new UserBook { Book = book, OwnerUserId = ownerUserId });
        await db.SaveChangesAsync();
        return book.Id;
    }

    private async Task<AppDbContext> CreateDbContextAsync()
    {
        var factory = _factory.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        return await factory.CreateDbContextAsync();
    }

    private static async Task WaitForRedisSubscribersAsync(IServer server, string channel, int expectedCount)
    {
        var stopWatch = Stopwatch.StartNew();
        while (stopWatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            var response = (RedisResult[]?)(await server.ExecuteAsync("PUBSUB", "NUMSUB", channel)) ?? [];
            if (response.Length > 1 && int.TryParse(response[1].ToString(), out var count) && count >= expectedCount)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        Assert.Fail($"Expected {expectedCount} Redis cache invalidation subscribers on the test channel.");
    }

    private static DefaultHttpContext CreateBookContext(string userId, string role)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/books";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId),
            new Claim(ClaimTypes.Role, role)
        ], "direct-test"));
        return context;
    }
}
