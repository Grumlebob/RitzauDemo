using RitzauDemo.Client.Features.Books.Shared;
using RitzauDemo.Core.Features.Books.UseCases.GetBooks;

namespace RitzauDemo.Client.Features.Books.UserBookcase;

internal static class UserBookcaseBookMapper
{
    public static IReadOnlyList<BookcaseBook> ToBookcaseBooks(IReadOnlyList<BookListItemResponse>? books)
    {
        if (books is not { Count: > 0 })
        {
            return [];
        }

        return books
            .Select(book => new BookcaseBook(book.Id, book.Title, book.Url, $"/books?bookId={book.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)}&bookMode=view"))
            .ToList();
    }
}
