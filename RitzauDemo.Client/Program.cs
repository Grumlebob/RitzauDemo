using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RitzauDemo.Client.Features.Books;
using RitzauDemo.Client.Features.Books.AuthorBookcase;
using RitzauDemo.Client.Features.Books.UserBookcase;
using RitzauDemo.Core.Features.Books.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(sp => new HttpClient(new BooksAntiforgeryHandler(sp.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>())
{
    InnerHandler = new HttpClientHandler()
})
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Books service for WASM after hydration
builder.Services.AddScoped<IBooksApi, BooksClientService>();
builder.Services.AddScoped<IAuthorBooksApi, AuthorBooksClientService>();
builder.Services.AddScoped<AuthorBookcaseState>();
builder.Services.AddScoped<UserBookcaseState>();

await builder.Build().RunAsync();
