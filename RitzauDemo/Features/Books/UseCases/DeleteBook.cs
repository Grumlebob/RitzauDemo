using Microsoft.AspNetCore.Http.HttpResults;
using RitzauDemo.Core.Features.Books.UseCases.DeleteBook;
using RitzauDemo.Features.Accounts.Authorization;
using RitzauDemo.Features.Accounts.Identity;
using RitzauDemo.Features.Books.Authorization;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;

namespace RitzauDemo.Features.Books.UseCases.DeleteBook;

internal sealed class DeleteBookHandler(
    IDbContextFactory<AppDbContext> dbFactory,
    BooksCachePolicy cachePolicy,
    BooksAuthorizationGuard authorization,
    ICurrentUserAccessor currentUser)
{
    public async Task<bool> HandleAsync(
        DeleteBookRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireAdminAsync(cancellationToken);
        var userId = await currentUser.GetRequiredUserIdAsync(cancellationToken);
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var book = await db.UserBooks
            .Where(userBook => userBook.BookId == request.Id && userBook.OwnerUserId == userId)
            .Select(userBook => userBook.Book)
            .FirstOrDefaultAsync(cancellationToken);
        if (book is null)
        {
            return false;
        }

        db.Books.Remove(book);
        await db.SaveChangesAsync(cancellationToken);
        await cachePolicy.InvalidateBookAsync(userId, request.Id);
        return true;
    }
}

internal static class DeleteBookEndpoint
{
    public static RouteHandlerBuilder MapDeleteBook(this RouteGroupBuilder group) =>
        group.MapDelete("/{id:int}", DeleteBookAsync)
            .WithName("DeleteBook")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithAuthorizationResponses(AccountAuthorizationPolicies.AdminOnly);

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteBookAsync(
        [AsParameters] DeleteBookRequest request,
        DeleteBookHandler handler,
        ILogger<BooksEndpointLogCategory> logger,
        CancellationToken cancellationToken)
    {
        const string operation = "delete";
        using var activity = BooksTelemetry.StartActivity(operation);
        var startTimestamp = BooksTelemetry.GetTimestamp();

        try
        {
            var success = await handler.HandleAsync(request, cancellationToken);
            if (!success)
            {
                logger.LogWarning("Book {BookId} not found for delete", request.Id);
                BooksTelemetry.Record(operation, "not_found", startTimestamp);
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Book not found",
                    detail: $"Book {request.Id} was not found.");
            }

            logger.LogInformation("Deleted book {BookId}", request.Id);
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
