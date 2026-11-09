using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Features.Books.AuthorBookcase.Seed;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;
using RitzauDemo.Features.Books.Services;

namespace RitzauDemo.Features.Books;

public static class DependencyInjection
{
    public static IServiceCollection AddBooksFeature(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IBooksApi, BooksServerService>();
        services.AddScoped<IAuthorBooksApi, AuthorBooksServerService>();
        services.AddScoped<IAuthorBookSeeder, AuthorBookSeeder>();
        services.Configure<BooksCacheOptions>(config.GetSection("Cache:Books"));
        return services;
    }

    public static IEndpointRouteBuilder MapBooksFeature(this IEndpointRouteBuilder routes)
    {
        routes.MapBookEndpoints();
        routes.MapAuthorBookEndpoints();
        return routes;
    }
}
