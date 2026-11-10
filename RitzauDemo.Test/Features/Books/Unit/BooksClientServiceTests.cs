using System.Text;
using RitzauDemo.Client.Features.Books;
using RitzauDemo.Core.Features.Books.UseCases.CreateBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBook;

namespace RitzauDemo.Test.Features.Books.Unit;

public sealed class BooksClientServiceTests
{
    [Fact]
    public async Task GetAsync_ThrowsWhenResponseHasNoListPayload()
    {
        var (service, client) = CreateService(HttpStatusCode.OK, "null");
        using (client)
        {
            var exception = await Assert.ThrowsAsync<InvalidDataException>(() => service.GetAsync());

            Assert.Equal("The Books API returned an empty list response.", exception.Message);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenSuccessfulResponseHasNoBookPayload()
    {
        var (service, client) = CreateService(HttpStatusCode.OK, "null");
        using (client)
        {
            var exception = await Assert.ThrowsAsync<InvalidDataException>(
                () => service.GetByIdAsync(new GetBookRequest { Id = 1 }));

            Assert.Equal("The Books API returned an empty book response.", exception.Message);
        }
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenSuccessfulResponseHasNoCreatedBookPayload()
    {
        var (service, client) = CreateService(HttpStatusCode.Created, "null");
        using (client)
        {
            var exception = await Assert.ThrowsAsync<InvalidDataException>(
                () => service.CreateAsync(new CreateBookRequest { Title = "A book" }));

            Assert.Equal("The Books API returned an empty create response.", exception.Message);
        }
    }

    private static (BooksClientService Service, HttpClient Client) CreateService(HttpStatusCode statusCode, string body)
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        return (new BooksClientService(client), client);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> createResponse) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(createResponse(request));
    }
}
