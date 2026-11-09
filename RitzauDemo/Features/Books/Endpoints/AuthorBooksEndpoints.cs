using RitzauDemo.Features.Books.UseCases.GetAuthorBook;
using RitzauDemo.Features.Books.UseCases.GetAuthorBooks;
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

        group.MapGetAuthorBooks();
        group.MapGetAuthorBook();
        return routes;
    }
}
