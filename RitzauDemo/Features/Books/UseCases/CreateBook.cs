using Microsoft.AspNetCore.Http.HttpResults;
using RitzauDemo.Core.Features.Books.Domain;
using RitzauDemo.Core.Features.Books.UseCases.CreateBook;
using RitzauDemo.Features.Accounts.Identity;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;

namespace RitzauDemo.Features.Books.UseCases.CreateBook;

internal sealed class CreateBookHandler(
    IDbContextFactory<AppDbContext> dbFactory,
    BooksCachePolicy cachePolicy,
    ICurrentUserAccessor currentUser)
{
    public async Task<CreateBookResponse> HandleAsync(
        CreateBookRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetRequiredUserIdAsync(cancellationToken);
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Url = NormalizeUrl(request.Url)
        };
        db.Books.Add(book);
        db.UserBooks.Add(new UserBook
        {
            Book = book,
            OwnerUserId = userId
        });
        await db.SaveChangesAsync(cancellationToken);
        await cachePolicy.InvalidateBookAsync(userId, book.Id);

        return new CreateBookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Url = book.Url
        };
    }

    private static string? NormalizeUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : url.Trim();
}

internal static class CreateBookEndpoint
{
    public static RouteHandlerBuilder MapCreateBook(this RouteGroupBuilder group) =>
        group.MapPost("/", CreateBookAsync)
            .WithName("CreateBook")
            .ProducesValidationProblem()
            .WithAuthorizationResponses();

    private static async Task<Created<CreateBookResponse>> CreateBookAsync(
        CreateBookRequest request,
        CreateBookHandler handler,
        ILogger<BooksEndpointLogCategory> logger,
        CancellationToken cancellationToken)
    {
        const string operation = "create";
        using var activity = BooksTelemetry.StartActivity(operation);
        var startTimestamp = BooksTelemetry.GetTimestamp();

        try
        {
            var response = await handler.HandleAsync(request, cancellationToken);
            logger.LogInformation("Created book {BookId}", response.Id);
            BooksTelemetry.Record(operation, "success", startTimestamp);
            return TypedResults.Created($"/api/books/{response.Id}", response);
        }
        catch (Exception exception)
        {
            BooksTelemetry.RecordException(operation, startTimestamp, exception);
            throw;
        }
    }
}
