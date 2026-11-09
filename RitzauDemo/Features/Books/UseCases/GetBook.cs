using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Hybrid;
using RitzauDemo.Core.Features.Books.UseCases.GetBook;
using RitzauDemo.Features.Accounts.Identity;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;

namespace RitzauDemo.Features.Books.UseCases.GetBook;

internal sealed class GetBookHandler(
    IDbContextFactory<AppDbContext> dbFactory,
    HybridCache cache,
    BooksCachePolicy cachePolicy,
    ICurrentUserAccessor currentUser)
{
    public async Task<GetBookResponse?> HandleAsync(
        GetBookRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetRequiredUserIdAsync(cancellationToken);
        return await cache.GetOrCreateAsync(
            BooksCacheKeys.Item(userId, request.Id),
            ct => new ValueTask<GetBookResponse?>(LoadBookAsync(userId, request.Id, ct)),
            cachePolicy.ItemEntryOptions(),
            tags: [BooksCacheKeys.AllTag, BooksCacheKeys.ItemTag(userId, request.Id)],
            cancellationToken: cancellationToken);
    }

    private async Task<GetBookResponse?> LoadBookAsync(
        string userId,
        int id,
        CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var book = await db.UserBooks
            .AsNoTracking()
            .Where(userBook => userBook.BookId == id && userBook.OwnerUserId == userId)
            .Select(userBook => userBook.Book)
            .FirstOrDefaultAsync(cancellationToken);
        if (book is null)
        {
            return null;
        }

        return new GetBookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Url = book.Url
        };
    }
}

internal static class GetBookEndpoint
{
    public static RouteHandlerBuilder MapGetBook(this RouteGroupBuilder group) =>
        group.MapGet("/{id:int}", GetBookAsync)
            .WithName("GetBook")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithAuthorizationResponses();

    private static async Task<Results<Ok<GetBookResponse>, ProblemHttpResult>> GetBookAsync(
        [AsParameters] GetBookRequest request,
        GetBookHandler handler,
        ILogger<BooksEndpointLogCategory> logger,
        CancellationToken cancellationToken)
    {
        const string operation = "get";
        using var activity = BooksTelemetry.StartActivity(operation);
        var startTimestamp = BooksTelemetry.GetTimestamp();

        try
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            if (result is null)
            {
                logger.LogWarning("Book {BookId} not found", request.Id);
                BooksTelemetry.Record(operation, "not_found", startTimestamp);
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Book not found",
                    detail: $"Book {request.Id} was not found.");
            }

            logger.LogInformation("Fetched book {BookId}", request.Id);
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
