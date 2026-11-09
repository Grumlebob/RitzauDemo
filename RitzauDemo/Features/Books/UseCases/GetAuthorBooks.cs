using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;
using RitzauDemo.Core.Features.Books.UseCases.GetAuthorBooks;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;

namespace RitzauDemo.Features.Books.UseCases.GetAuthorBooks;

internal sealed class GetAuthorBooksHandler(IDbContextFactory<AppDbContext> dbFactory, HybridCache cache)
{
    public async Task<GetAuthorBooksResponse> HandleAsync(CancellationToken cancellationToken = default)
    {
        var result = await cache.GetOrCreateAsync(
            AuthorBooksCacheKeys.ListKey,
            ct => new ValueTask<GetAuthorBooksResponse>(LoadBooksAsync(ct)),
            PublicEntryOptions(),
            tags: [AuthorBooksCacheKeys.AllTag, AuthorBooksCacheKeys.ListTag],
            cancellationToken: cancellationToken);

        return result!;
    }

    private async Task<GetAuthorBooksResponse> LoadBooksAsync(CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var items = await db.AuthorBooks
            .AsNoTracking()
            .OrderBy(authorBook => authorBook.BookId)
            .Select(authorBook => new AuthorBookListItemResponse
            {
                Id = authorBook.Book.Id,
                SeedKey = authorBook.SeedKey,
                Title = authorBook.Book.Title,
                Author = authorBook.Book.Author,
                Url = authorBook.Book.Url
            })
            .ToListAsync(cancellationToken);

        return new GetAuthorBooksResponse { Books = items };
    }

    private static HybridCacheEntryOptions PublicEntryOptions() => new()
    {
        Expiration = TimeSpan.FromMinutes(60),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };
}

internal static class GetAuthorBooksEndpoint
{
    public static RouteHandlerBuilder MapGetAuthorBooks(this RouteGroupBuilder group) =>
        group.MapGet("/", ListAuthorBooksAsync)
            .WithName("ListAuthorBooks");

    private static async Task<Ok<GetAuthorBooksResponse>> ListAuthorBooksAsync(
        GetAuthorBooksHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);
        return TypedResults.Ok(result);
    }
}
