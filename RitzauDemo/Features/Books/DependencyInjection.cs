using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Features.Books.AuthorBookcase.Seed;
using RitzauDemo.Features.Books.Caching;
using RitzauDemo.Features.Books.Endpoints;
using RitzauDemo.Features.Books.Services;
using RitzauDemo.Features.Books.UseCases.CreateBook;
using RitzauDemo.Features.Books.UseCases.DeleteBook;
using RitzauDemo.Features.Books.UseCases.GetAuthorBook;
using RitzauDemo.Features.Books.UseCases.GetAuthorBooks;
using RitzauDemo.Features.Books.UseCases.GetBook;
using RitzauDemo.Features.Books.UseCases.GetBooks;
using RitzauDemo.Features.Books.UseCases.UpdateBook;

namespace RitzauDemo.Features.Books;

public static class DependencyInjection
{
    public static IServiceCollection AddBooksFeature(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<BooksCachePolicy>();
        services.AddScoped<GetBooksHandler>();
        services.AddScoped<GetBookHandler>();
        services.AddScoped<CreateBookHandler>();
        services.AddScoped<UpdateBookHandler>();
        services.AddScoped<DeleteBookHandler>();
        services.AddScoped<GetAuthorBooksHandler>();
        services.AddScoped<GetAuthorBookHandler>();
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
