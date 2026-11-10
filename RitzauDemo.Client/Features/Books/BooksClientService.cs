using System.Net;
using RitzauDemo.Core.Features.Books.Contracts;
using RitzauDemo.Core.Features.Books.UseCases.CreateBook;
using RitzauDemo.Core.Features.Books.UseCases.DeleteBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBooks;
using RitzauDemo.Core.Features.Books.UseCases.UpdateBook;

namespace RitzauDemo.Client.Features.Books;

public class BooksClientService(HttpClient http) : IBooksApi
{
    private readonly HttpClient _http = http;

    public async Task<GetBooksResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var res = await _http.GetFromJsonAsync<GetBooksResponse>("api/books", cancellationToken);
        return res ?? throw new InvalidDataException("The Books API returned an empty list response.");
    }

    public async Task<GetBookResponse?> GetByIdAsync(GetBookRequest req, CancellationToken cancellationToken = default)
    {
        using var res = await _http.GetAsync($"api/books/{req.Id}", cancellationToken);
        if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<GetBookResponse>(cancellationToken)
            ?? throw new InvalidDataException("The Books API returned an empty book response.");
    }

    public async Task<CreateBookResponse> CreateAsync(CreateBookRequest req, CancellationToken cancellationToken = default)
    {
        using var res = await _http.PostAsJsonAsync("api/books", req, cancellationToken);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<CreateBookResponse>(cancellationToken)
            ?? throw new InvalidDataException("The Books API returned an empty create response.");
    }

    public async Task<bool> UpdateAsync(UpdateBookRequest req, CancellationToken cancellationToken = default)
    {
        using var res = await _http.PutAsJsonAsync($"api/books/{req.Id}", req, cancellationToken);
        if (res.StatusCode == HttpStatusCode.NoContent) return true;
        if (res.StatusCode == HttpStatusCode.NotFound) return false;
        res.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> DeleteAsync(DeleteBookRequest req, CancellationToken cancellationToken = default)
    {
        using var res = await _http.DeleteAsync($"api/books/{req.Id}", cancellationToken);
        if (res.StatusCode == HttpStatusCode.NoContent) return true;
        if (res.StatusCode == HttpStatusCode.NotFound) return false;
        res.EnsureSuccessStatusCode();
        return true;
    }
}
