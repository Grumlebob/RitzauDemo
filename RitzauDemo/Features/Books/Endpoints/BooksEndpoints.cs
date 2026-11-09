using RitzauDemo.Features.Accounts;
using RitzauDemo.Features.Accounts.Security;
using RitzauDemo.Features.Books.UseCases.CreateBook;
using RitzauDemo.Features.Books.UseCases.DeleteBook;
using RitzauDemo.Features.Books.UseCases.GetBook;
using RitzauDemo.Features.Books.UseCases.GetBooks;
using RitzauDemo.Features.Books.UseCases.UpdateBook;
using RitzauDemo.Infrastructure.Hosting;

namespace RitzauDemo.Features.Books.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/books")
            .WithTags("Books")
            .RequireRateLimiting(AppRateLimiting.ApiPolicyName)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithMetadata(new ExpectedAccountUser())
            .WithMetadata(new AccountRequestProtection())
            .WithPrivateNoStoreResponses();

        group.MapGetBooks();
        group.MapGetBook();
        group.MapCreateBook();
        group.MapUpdateBook();
        group.MapDeleteBook();
        return routes;
    }
}

internal static class BookEndpointMetadata
{
    public static RouteHandlerBuilder WithAuthorizationResponses(this RouteHandlerBuilder endpoint) =>
        endpoint
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization();
}

internal sealed class BooksEndpointLogCategory;
