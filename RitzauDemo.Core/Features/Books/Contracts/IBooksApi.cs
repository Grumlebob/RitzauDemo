using RitzauDemo.Core.Features.Books.UseCases.CreateBook;
using RitzauDemo.Core.Features.Books.UseCases.DeleteBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBooks;
using RitzauDemo.Core.Features.Books.UseCases.UpdateBook;

namespace RitzauDemo.Core.Features.Books.Contracts;

public interface IBooksApi
{
    Task<GetBooksResponse> GetAsync(CancellationToken cancellationToken = default);
    Task<GetBookResponse?> GetByIdAsync(GetBookRequest req, CancellationToken cancellationToken = default);
    Task<CreateBookResponse> CreateAsync(CreateBookRequest req, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateBookRequest req, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(DeleteBookRequest req, CancellationToken cancellationToken = default);
}
