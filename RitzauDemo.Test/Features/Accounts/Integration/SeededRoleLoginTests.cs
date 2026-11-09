using RitzauDemo.Core.Features.Accounts.Contracts;
using RitzauDemo.Core.Features.Books.UseCases.CreateBook;
using RitzauDemo.Core.Features.Books.UseCases.GetBooks;
using RitzauDemo.Features.Books.UserBookcase.Seed;
using RitzauDemo.Test.TestSupport.Integration;

namespace RitzauDemo.Test.Features.Accounts.Integration;

[Collection(TestCollectionNames.SeededAccounts)]
public sealed class SeededRoleLoginTests(SeededAccountsWebAppFactory factory)
{
    [Fact]
    public async Task AccountSurfaceOnlyExposesSeededSignInAndSignOut()
    {
        var client = factory.HttpClient;
        using var login = await client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var loginPage = await login.Content.ReadAsStringAsync();
        Assert.Contains("Sign in with your account", loginPage, StringComparison.Ordinal);
        Assert.DoesNotContain("Account/Register", loginPage, StringComparison.Ordinal);
        Assert.DoesNotContain("ForgotPassword", loginPage, StringComparison.Ordinal);
        Assert.DoesNotContain("Passkey", loginPage, StringComparison.Ordinal);
        Assert.DoesNotContain("ExternalLogin", loginPage, StringComparison.Ordinal);

        var lockoutPage = await client.GetStringAsync("/Account/Lockout");
        Assert.Contains("account has been locked out", lockoutPage, StringComparison.OrdinalIgnoreCase);
        var accessDeniedPage = await client.GetStringAsync("/Account/AccessDenied");
        Assert.Contains("You do not have access", accessDeniedPage, StringComparison.Ordinal);

        foreach (var path in new[]
        {
            "/api/auth/register",
            "/api/auth/forgot-password",
            "/api/auth/reset-password",
            "/api/auth/change-password",
            "/api/auth/change-email",
            "/api/auth/export",
            "/api/auth/delete",
            "/Account/Register",
            "/Account/Manage",
            "/Account/LoginWith2fa"
        })
        {
            using var response = path.StartsWith("/api/", StringComparison.Ordinal)
                ? await client.PostAsync(path, JsonContent.Create(new { }))
                : await client.GetAsync(path);
            Assert.True(response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
                $"Expected {path} to be absent, received {(int)response.StatusCode}.");
        }
    }

    [Fact]
    public async Task SeededUserAndAdminLoginWithDifferentBookPermissions()
    {
        var client = factory.HttpClient;
        var user = await SignInAsync(client, SeededAccountsWebAppFactory.UserEmail, factory.UserPassword);

        Assert.NotNull(user.User);
        Assert.Equal(AccountRoleNames.User, Assert.Single(user.User!.Roles));

        using var userBooksResponse = await GetBooksAsync(client, user);
        Assert.Equal(HttpStatusCode.OK, userBooksResponse.StatusCode);
        var userBooks = await userBooksResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(userBooks);
        Assert.Equal(2, userBooks!.Books.Count);

        using (var scope = factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<DemoUserBooksSeeder>().SeedAsync();
        }
        using var repeatedSeedResponse = await GetBooksAsync(client, user);
        var repeatedSeed = await repeatedSeedResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(repeatedSeed);
        Assert.Equal(2, repeatedSeed!.Books.Count);

        using var userCreate = await PostBookAsync(client, user, new CreateBookRequest
        {
            Title = "User write should be denied",
            Author = "Reader"
        });
        Assert.True(userCreate.StatusCode == HttpStatusCode.Forbidden,
            $"Expected a forbidden response, received {(int)userCreate.StatusCode}: {await userCreate.Content.ReadAsStringAsync()}");

        var userCreatePage = await client.GetStringAsync("/books?bookMode=create");
        Assert.Contains("Administrator access required", userCreatePage, StringComparison.Ordinal);
        Assert.DoesNotContain("data-testid=\"book-save\"", userCreatePage, StringComparison.Ordinal);

        using var logout = await PostLogoutAsync(client, user);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        var signedOut = await ReadSessionAsync(client);
        Assert.Null(signedOut.User);
        using var anonymousBooks = await client.GetAsync("/api/books");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousBooks.StatusCode);

        var admin = await SignInAsync(client, SeededAccountsWebAppFactory.AdminEmail, factory.AdminPassword);
        Assert.NotNull(admin.User);
        Assert.Equal(AccountRoleNames.Admin, Assert.Single(admin.User!.Roles));

        var adminCreatePage = await client.GetStringAsync("/books?bookMode=create");
        Assert.Contains("data-testid=\"book-save\"", adminCreatePage, StringComparison.Ordinal);

        using var adminCreate = await PostBookAsync(client, admin, new CreateBookRequest
        {
            Title = "Admin write succeeds",
            Author = "Administrator"
        });
        Assert.Equal(HttpStatusCode.Created, adminCreate.StatusCode);

        using var adminBooksResponse = await GetBooksAsync(client, admin);
        Assert.Equal(HttpStatusCode.OK, adminBooksResponse.StatusCode);
        var adminBooks = await adminBooksResponse.Content.ReadFromJsonAsync<GetBooksResponse>();
        Assert.NotNull(adminBooks);
        Assert.Equal("Admin write succeeds", Assert.Single(adminBooks!.Books).Title);
    }

    private static async Task<AccountSession> SignInAsync(HttpClient client, string email, string password)
    {
        var anonymousSession = await ReadSessionAsync(client);
        using var login = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new AccountLoginRequest
            {
                Email = email,
                Password = password
            })
        };
        login.Headers.Add(TestAccountHeadersHandler.CsrfHeader, anonymousSession.CsrfToken);
        using var loginResponse = await client.SendAsync(login);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        return await ReadSessionAsync(client);
    }

    private static async Task<AccountSession> ReadSessionAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/auth/session");
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Account session returned {(int)response.StatusCode}.");
        }

        return (await response.Content.ReadFromJsonAsync<AccountSession>()) ??
            throw new InvalidOperationException("The account session response was empty.");
    }

    private static async Task<HttpResponseMessage> PostBookAsync(
        HttpClient client,
        AccountSession session,
        CreateBookRequest request)
    {
        var user = session.User ?? throw new InvalidOperationException("A signed-in account is required.");
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/books")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add(TestAccountHeadersHandler.CsrfHeader, session.CsrfToken);
        message.Headers.Add(TestAccountHeadersHandler.ExpectedUserHeader, user.Id);
        return await client.SendAsync(message);
    }

    private static Task<HttpResponseMessage> GetBooksAsync(HttpClient client, AccountSession session)
    {
        var user = session.User ?? throw new InvalidOperationException("A signed-in account is required.");
        var message = new HttpRequestMessage(HttpMethod.Get, "/api/books");
        message.Headers.Add(TestAccountHeadersHandler.ExpectedUserHeader, user.Id);
        return client.SendAsync(message);
    }

    private static Task<HttpResponseMessage> PostLogoutAsync(HttpClient client, AccountSession session)
    {
        var user = session.User ?? throw new InvalidOperationException("A signed-in account is required.");
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
        {
            Content = JsonContent.Create(new { })
        };
        message.Headers.Add(TestAccountHeadersHandler.CsrfHeader, session.CsrfToken);
        message.Headers.Add(TestAccountHeadersHandler.ExpectedUserHeader, user.Id);
        return client.SendAsync(message);
    }
}

[CollectionDefinition(TestCollectionNames.SeededAccounts, DisableParallelization = true)]
public sealed class SeededAccountsCollection : ICollectionFixture<SeededAccountsWebAppFactory>;

public sealed class SeededAccountsWebAppFactory : WebAppFactory
{
    public const string AdminEmail = "admin-seed@example.test";
    public const string UserEmail = "user-seed@example.test";

    private readonly string _adminPassword;
    private readonly string _userPassword;

    public string AdminPassword => _adminPassword;
    public string UserPassword => _userPassword;

    public SeededAccountsWebAppFactory()
        : this(CreateConfiguration())
    {
    }

    private SeededAccountsWebAppFactory(SeededAccountsConfiguration configuration)
        : base(configuration.Options)
    {
        _adminPassword = configuration.AdminPassword;
        _userPassword = configuration.UserPassword;
    }

    private static SeededAccountsConfiguration CreateConfiguration()
    {
        var adminPassword = $"Admin-{Guid.NewGuid():N}-R7!";
        var userPassword = $"User-{Guid.NewGuid():N}-C4!";
        var options = new WebAppFactoryOptions
        {
            UseRealIdentityCookies = true,
            ConfigurationOverrides = new Dictionary<string, string?>
            {
                ["Accounts:AdminSeed:Enabled"] = "true",
                ["Accounts:AdminSeed:Email"] = AdminEmail,
                ["Accounts:AdminSeed:Password"] = adminPassword,
                ["Accounts:UserSeed:Enabled"] = "true",
                ["Accounts:UserSeed:Email"] = UserEmail,
                ["Accounts:UserSeed:Password"] = userPassword
            }
        };
        return new SeededAccountsConfiguration(options, adminPassword, userPassword);
    }

    private sealed record SeededAccountsConfiguration(
        WebAppFactoryOptions Options,
        string AdminPassword,
        string UserPassword);
}
