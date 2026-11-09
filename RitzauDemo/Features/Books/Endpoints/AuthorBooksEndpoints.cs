using Microsoft.AspNetCore.Http.HttpResults;
using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Core.Features.Books.UseCases.GetAuthorBook;
using RitzauDemo.Core.Features.Books.UseCases.GetAuthorBooks;
using RitzauDemo.Infrastructure.Hosting;

namespace RitzauDemo.Features.Books.Endpoints;

public static class AuthorBooksEndpoints
{
    public static IEndpointRouteBuilder MapAuthorBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/author-books")
            .WithTags("Author Books")
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(AppRateLimiting.ApiPolicyName);

        group.MapGet("/", ListAuthorBooksAsync)
            .WithName("ListAuthorBooks");

        group.MapGet("/{id:int}", GetAuthorBookAsync)
            .WithName("GetAuthorBook")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<Ok<GetAuthorBooksResponse>> ListAuthorBooksAsync(
        IAuthorBooksApi authorBooks,
        CancellationToken cancellationToken)
    {
        var result = await authorBooks.GetAsync(cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<GetAuthorBookResponse>, ProblemHttpResult>> GetAuthorBookAsync(
        [AsParameters] GetAuthorBookRequest req,
        IAuthorBooksApi authorBooks,
        CancellationToken cancellationToken)
    {
        var result = await authorBooks.GetByIdAsync(req, cancellationToken);
        return result is null
            ? TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Author book not found",
                detail: $"Author book {req.Id} was not found.")
            : TypedResults.Ok(result);
    }
}
