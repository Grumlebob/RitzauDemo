using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Core.Features.Books.UseCases.GetAuthorBook;
using RitzauDemo.Core.Features.Books.UseCases.GetAuthorBooks;
using RitzauDemo.Features.Books.UseCases.GetAuthorBook;
using RitzauDemo.Features.Books.UseCases.GetAuthorBooks;

namespace RitzauDemo.Features.Books.Services;

internal sealed class AuthorBooksServerService(
    GetAuthorBooksHandler getAuthorBooks,
    GetAuthorBookHandler getAuthorBook) : IAuthorBooksApi
{
    public Task<GetAuthorBooksResponse> GetAsync(CancellationToken cancellationToken = default) =>
        getAuthorBooks.HandleAsync(cancellationToken);

    public Task<GetAuthorBookResponse?> GetByIdAsync(
        GetAuthorBookRequest request,
        CancellationToken cancellationToken = default) =>
        getAuthorBook.HandleAsync(request, cancellationToken);
}
