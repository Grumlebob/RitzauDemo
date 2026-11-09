using Microsoft.AspNetCore.Http.HttpResults;
using RitzauDemo.Core.Features.Books.UseCases.UpdateBook;
using RitzauDemo.Features.Accounts.Identity;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;

namespace RitzauDemo.Features.Books.UseCases.UpdateBook;

internal sealed class UpdateBookHandler(
    IDbContextFactory<AppDbContext> dbFactory,
    BooksCachePolicy cachePolicy,
    ICurrentUserAccessor currentUser)
{
    public async Task<bool> HandleAsync(
        UpdateBookRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetRequiredUserIdAsync(cancellationToken);
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var userBook = await db.UserBooks
            .Include(ownerBook => ownerBook.Book)
            .FirstOrDefaultAsync(
                ownerBook => ownerBook.BookId == request.Id && ownerBook.OwnerUserId == userId,
                cancellationToken);
        var book = userBook?.Book;
        if (book is null)
        {
            return false;
        }

        book.Title = request.Title;
        book.Author = request.Author;
        book.Url = NormalizeUrl(request.Url);
        await db.SaveChangesAsync(cancellationToken);
        await cachePolicy.InvalidateBookAsync(userId, request.Id);
        return true;
    }

    private static string? NormalizeUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : url.Trim();
}

internal static class UpdateBookEndpoint
{
    public static RouteHandlerBuilder MapUpdateBook(this RouteGroupBuilder group) =>
        group.MapPut("/{id:int}", UpdateBookAsync)
            .WithName("UpdateBook")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithAuthorizationResponses();

    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateBookAsync(
        int id,
        UpdateBookRequest request,
        UpdateBookHandler handler,
        ILogger<BooksEndpointLogCategory> logger,
        CancellationToken cancellationToken)
    {
        const string operation = "update";
        using var activity = BooksTelemetry.StartActivity(operation);
        var startTimestamp = BooksTelemetry.GetTimestamp();

        if (id != request.Id)
        {
            logger.LogWarning("Update mismatch: route id {RouteId} != body id {BodyId}", id, request.Id);
            BooksTelemetry.Record(operation, "bad_request", startTimestamp);
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Book id mismatch",
                detail: "The route id and body id do not match.");
        }

        try
        {
            var success = await handler.HandleAsync(request, cancellationToken);
            if (!success)
            {
                logger.LogWarning("Book {BookId} not found for update", request.Id);
                BooksTelemetry.Record(operation, "not_found", startTimestamp);
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Book not found",
                    detail: $"Book {request.Id} was not found.");
            }

            logger.LogInformation("Updated book {BookId}", request.Id);
            BooksTelemetry.Record(operation, "success", startTimestamp);
            return TypedResults.NoContent();
        }
        catch (Exception exception)
        {
            BooksTelemetry.RecordException(operation, startTimestamp, exception);
            throw;
        }
    }
}
