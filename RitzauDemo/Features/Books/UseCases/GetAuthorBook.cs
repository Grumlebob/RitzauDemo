using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;
using RitzauDemo.Core.Features.Books.UseCases.GetAuthorBook;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;

namespace RitzauDemo.Features.Books.UseCases.GetAuthorBook;

internal sealed class GetAuthorBookHandler(IDbContextFactory<AppDbContext> dbFactory, HybridCache cache)
{
    public async Task<GetAuthorBookResponse?> HandleAsync(
        GetAuthorBookRequest request,
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            AuthorBooksCacheKeys.Item(request.Id),
            ct => new ValueTask<GetAuthorBookResponse?>(LoadBookAsync(request.Id, ct)),
            PublicEntryOptions(),
            tags: [AuthorBooksCacheKeys.AllTag, AuthorBooksCacheKeys.ItemTag(request.Id)],
            cancellationToken: cancellationToken);
    }

    private async Task<GetAuthorBookResponse?> LoadBookAsync(int id, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.AuthorBooks
            .AsNoTracking()
            .Where(authorBook => authorBook.BookId == id)
            .Select(authorBook => new GetAuthorBookResponse
            {
                Id = authorBook.Book.Id,
                SeedKey = authorBook.SeedKey,
                Title = authorBook.Book.Title,
                Author = authorBook.Book.Author,
                Url = authorBook.Book.Url
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static HybridCacheEntryOptions PublicEntryOptions() => new()
    {
        Expiration = TimeSpan.FromMinutes(60),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };
}

internal static class GetAuthorBookEndpoint
{
    public static RouteHandlerBuilder MapGetAuthorBook(this RouteGroupBuilder group) =>
        group.MapGet("/{id:int}", GetAuthorBookAsync)
            .WithName("GetAuthorBook")
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<Results<Ok<GetAuthorBookResponse>, ProblemHttpResult>> GetAuthorBookAsync(
        [AsParameters] GetAuthorBookRequest request,
        GetAuthorBookHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result is null
            ? TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Author book not found",
                detail: $"Author book {request.Id} was not found.")
            : TypedResults.Ok(result);
    }
}
