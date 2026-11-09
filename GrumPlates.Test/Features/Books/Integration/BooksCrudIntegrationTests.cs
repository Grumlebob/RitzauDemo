using GrumPlates.Core.Features.Books.UseCases.CreateBook;
using GrumPlates.Core.Features.Books.UseCases.GetBook;
using GrumPlates.Core.Features.Books.UseCases.GetBooks;
using GrumPlates.Core.Features.Books.UseCases.UpdateBook;
using GrumPlates.Test.TestSupport.Integration;

namespace GrumPlates.Test.Features.Books.Integration;

[Collection(TestCollectionNames.Integration)]
public sealed class BooksCrudIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _ownerClient;
    private readonly HttpClient _otherUserClient;
    private readonly Func<Task> _resetDatabase;

    public BooksCrudIntegrationTests(WebAppFactory factory)
    {
        _ownerClient = factory.CreateAuthenticatedClient("crud-owner@example.test");
        _otherUserClient = factory.CreateAuthenticatedClient("other-owner@example.test");
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    [Fact]
    public async Task OwnerCanCreateListReadUpdateAndDeleteBook()
    {
        var create = new CreateBookRequest
        {
            Title = "CRUD demo book",
            Author = "Demo author",
            Url = "https://example.test/books/crud-demo"
        };

        using var createResponse = await _ownerClient.PostAsJsonAsync("/api/books", create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateBookResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);

        using var listResponse = await _ownerClient.GetAsync("/api/books");
        listResponse.EnsureSuccessStatusCode();
        var listed = await listResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(listed);
        var listedBook = Assert.Single(listed!.Books, book => book.Id == created.Id);
        Assert.Equal(create.Title, listedBook.Title);

        using var otherUserResponse = await _otherUserClient.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, otherUserResponse.StatusCode);

        var update = new UpdateBookRequest
        {
            Id = created.Id,
            Title = "CRUD demo book updated",
            Author = create.Author,
            Url = create.Url
        };
        using var updateResponse = await _ownerClient.PutAsJsonAsync($"/api/books/{created.Id}", update);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        using var readResponse = await _ownerClient.GetAsync($"/api/books/{created.Id}");
        readResponse.EnsureSuccessStatusCode();
        var read = await readResponse.Content.ReadFromJsonAsync<GetBookResponse>();
        Assert.NotNull(read);
        Assert.Equal(update.Title, read!.Title);

        using var deleteResponse = await _ownerClient.DeleteAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var missingResponse = await _ownerClient.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);

        using var finalListResponse = await _ownerClient.GetAsync("/api/books");
        finalListResponse.EnsureSuccessStatusCode();
        var finalList = await finalListResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(finalList);
        Assert.Empty(finalList!.Books);
    }

    public async ValueTask InitializeAsync() => await _resetDatabase();

    public async ValueTask DisposeAsync() => await _resetDatabase();
}
