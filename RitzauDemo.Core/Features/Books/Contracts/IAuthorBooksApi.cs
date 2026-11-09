using RitzauDemo.Core.Features.Books.UseCases.GetAuthorBook;
using RitzauDemo.Core.Features.Books.UseCases.GetAuthorBooks;

namespace RitzauDemo.Core.Features.Books.Contracts;

public interface IAuthorBooksApi
{
    Task<GetAuthorBooksResponse> GetAsync(CancellationToken cancellationToken = default);

    Task<GetAuthorBookResponse?> GetByIdAsync(GetAuthorBookRequest req, CancellationToken cancellationToken = default);
}
