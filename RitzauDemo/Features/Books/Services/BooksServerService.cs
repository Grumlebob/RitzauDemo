using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Core.Features.Books.UseCases.CreateBook;
using RitzauDemo.Core.Features.Books.UseCases.DeleteBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBooks;
using RitzauDemo.Core.Features.Books.UseCases.UpdateBook;
using RitzauDemo.Features.Books.UseCases.CreateBook;
using RitzauDemo.Features.Books.UseCases.DeleteBook;
using RitzauDemo.Features.Books.UseCases.GetBook;
using RitzauDemo.Features.Books.UseCases.GetBooks;
using RitzauDemo.Features.Books.UseCases.UpdateBook;

namespace RitzauDemo.Features.Books.Services;

internal sealed class BooksServerService(
    GetBooksHandler getBooks,
    GetBookHandler getBook,
    CreateBookHandler createBook,
    UpdateBookHandler updateBook,
    DeleteBookHandler deleteBook) : IBooksApi
{
    public Task<GetBooksResponse> GetAsync(CancellationToken cancellationToken = default) =>
        getBooks.HandleAsync(cancellationToken);

    public Task<GetBookResponse?> GetByIdAsync(
        GetBookRequest request,
        CancellationToken cancellationToken = default) =>
        getBook.HandleAsync(request, cancellationToken);

    public Task<CreateBookResponse> CreateAsync(
        CreateBookRequest request,
        CancellationToken cancellationToken = default) =>
        createBook.HandleAsync(request, cancellationToken);

    public Task<bool> UpdateAsync(
        UpdateBookRequest request,
        CancellationToken cancellationToken = default) =>
        updateBook.HandleAsync(request, cancellationToken);

    public Task<bool> DeleteAsync(
        DeleteBookRequest request,
        CancellationToken cancellationToken = default) =>
        deleteBook.HandleAsync(request, cancellationToken);
}
