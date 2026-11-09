using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;
using RitzauDemo.Core.Features.Books.UseCases.GetBooks;
using RitzauDemo.Features.Accounts.Authorization;
using RitzauDemo.Features.Accounts.Identity;
using RitzauDemo.Features.Books.Authorization;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;

namespace RitzauDemo.Features.Books.UseCases.GetBooks;

internal sealed class GetBooksHandler(
    IDbContextFactory<AppDbContext> dbFactory,
    HybridCache cache,
    BooksCachePolicy cachePolicy,
    BooksAuthorizationGuard authorization,
    ICurrentUserAccessor currentUser)
{
    public async Task<GetBooksResponse> HandleAsync(CancellationToken cancellationToken = default)
    {
        await authorization.RequireReadAsync(cancellationToken);
        var userId = await currentUser.GetRequiredUserIdAsync(cancellationToken);
        var result = await cache.GetOrCreateAsync(
            BooksCacheKeys.List(userId),
            ct => new ValueTask<GetBooksResponse>(LoadBooksAsync(userId, ct)),
            cachePolicy.ListEntryOptions(),
            tags: [BooksCacheKeys.AllTag, BooksCacheKeys.ListTag(userId)],
            cancellationToken: cancellationToken);

        return result!;
    }

    private async Task<GetBooksResponse> LoadBooksAsync(string userId, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var items = await db.UserBooks
            .AsNoTracking()
            .Where(userBook => userBook.OwnerUserId == userId)
            .OrderBy(userBook => userBook.BookId)
            .Select(userBook => new BookListItemResponse
            {
                Id = userBook.Book.Id,
                Title = userBook.Book.Title,
                Author = userBook.Book.Author,
                Url = userBook.Book.Url
            })
            .ToListAsync(cancellationToken);

        return new GetBooksResponse { Books = items };
    }
}

internal static class GetBooksEndpoint
{
    public static RouteHandlerBuilder MapGetBooks(this RouteGroupBuilder group) =>
        group.MapGet("/", ListBooksAsync)
            .WithName("ListBooks")
            .WithAuthorizationResponses(AccountAuthorizationPolicies.BooksRead);

    private static async Task<Ok<GetBooksResponse>> ListBooksAsync(
        GetBooksHandler handler,
        ILogger<BooksEndpointLogCategory> logger,
        CancellationToken cancellationToken)
    {
        const string operation = "list";
        using var activity = BooksTelemetry.StartActivity(operation);
        var startTimestamp = BooksTelemetry.GetTimestamp();

        try
        {
            logger.LogDebug("Listing books");
            var result = await handler.HandleAsync(cancellationToken);
            BooksTelemetry.Record(operation, "success", startTimestamp);
            return TypedResults.Ok(result);
        }
        catch (Exception exception)
        {
            BooksTelemetry.RecordException(operation, startTimestamp, exception);
            throw;
        }
    }
}
