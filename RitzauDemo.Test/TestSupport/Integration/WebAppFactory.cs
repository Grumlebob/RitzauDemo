using System.Runtime.ExceptionServices;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using RitzauDemo.Features.Books.Caching;
using Testcontainers.PostgreSql;

namespace RitzauDemo.Test.TestSupport.Integration;

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const int MaxWaitTimeMinutes = 5;
    // Integration tests share one client IP; generous defaults keep unrelated tests
    // from tripping limits. Rate-limit tests set low limits explicitly.
    private const int DefaultGlobalPermitLimit = 10_000;
    private const int DefaultApiPermitLimit = 1_000;
    private const string RyukImageEnvironmentVariable = "TESTCONTAINERS_RYUK_CONTAINER_IMAGE";
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__DefaultConnection";
    private const string RedisConfigurationEnvironmentVariable = "Redis__Configuration";
    private const string RedisAllowMissingEnvironmentVariable = "Redis__AllowMissing";
    private const string AppNameEnvironmentVariable = "App__Name";
    private const string CacheInvalidationEnabledEnvironmentVariable = "Cache__Invalidation__Enabled";
    private const string CacheInvalidationNodeIdEnvironmentVariable = "Cache__Invalidation__NodeId";
    private const string CacheInvalidationChannelEnvironmentVariable = "Cache__Invalidation__ChannelName";
    private const string CacheBooksLocalListTtlEnvironmentVariable = "Cache__Books__LocalListTtlSeconds";
    private const string CacheBooksLocalItemTtlEnvironmentVariable = "Cache__Books__LocalItemTtlSeconds";
    private const string CacheBooksDisableLocalEnvironmentVariable = "Cache__Books__DisableLocalCache";
    private const string StartupMigrationsEnvironmentVariable = "Database__RunMigrationsAtStartup";
    private const string ForwardedHeaderKnownNetworkV4EnvironmentVariable = "ForwardedHeaders__KnownNetworks__0";
    private const string ForwardedHeaderKnownNetworkV6EnvironmentVariable = "ForwardedHeaders__KnownNetworks__1";
    private const string GlobalRateLimitEnvironmentVariable = "RateLimiting__Global__PermitLimit";
    private const string ApiRateLimitEnvironmentVariable = "RateLimiting__Api__PermitLimit";
    private const string AuthorBooksSeedAtStartupEnvironmentVariable = "AuthorBooks__SeedAtStartup";
    private const string OpenTelemetryEnabledEnvironmentVariable = "Observability__OpenTelemetry__Enabled";
    private const string OpenTelemetryEndpointEnvironmentVariable = "Observability__OpenTelemetry__Endpoint";
    static WebAppFactory()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(RyukImageEnvironmentVariable)))
        {
            Environment.SetEnvironmentVariable(RyukImageEnvironmentVariable, TestContainerImages.Ryuk);
        }
    }

    private readonly WebAppFactoryOptions _options;
    private readonly PostgreSqlContainer? _dbContainer;
    private readonly IContainer? _redisContainer;
    private readonly string _cacheInvalidationNodeId;
    private readonly string _cacheInvalidationChannelName;

    private string _connectionString = default!;
    private string _redisConnectionString = "CHANGE_ME";
    private Respawner? _respawner;
    private EnvironmentVariableScope? _environmentOverrides;
    private readonly TestKeyEncryptionCertificate _keyCertificate = new();
    private int _disposed;
    public HttpClient HttpClient { get; private set; } = default!;
    internal string ConnectionString => _connectionString;
    internal string RedisConnectionString => _redisConnectionString;
    internal string CacheInvalidationChannelName => _cacheInvalidationChannelName;
    internal string KeyEncryptionCertificatePath => _keyCertificate.Path;

    public WebAppFactory()
        : this(new WebAppFactoryOptions
        {
            LocalListTtlSeconds = 60,
            LocalItemTtlSeconds = 60
        })
    {
    }

    internal WebAppFactory(WebAppFactoryOptions options)
    {
        _options = options;
        _cacheInvalidationNodeId = options.CacheInvalidationNodeId ?? $"test-node-{Guid.NewGuid():N}";
        _cacheInvalidationChannelName = options.CacheInvalidationChannelName ?? $"test-channel-{Guid.NewGuid():N}";
        // Account antiforgery cookies are Secure-only, as behind the deployed HTTPS proxy.
        ClientOptions.BaseAddress = new Uri("https://localhost");
        if (string.IsNullOrWhiteSpace(options.PostgresConnectionString))
        {
            _dbContainer = TestContainerImages.CreatePostgreSqlFixture()
                .WithLabel(TestContainerLabels.For("web-app-factory-postgres"))
                .WithCleanUp(true)
                .Build();
        }
        if (string.IsNullOrWhiteSpace(options.RedisConnectionString))
        {
            _redisContainer = TestContainerImages.CreateRedisFixture()
                .WithLabel(TestContainerLabels.For("web-app-factory-redis"))
                .WithCleanUp(true)
                .Build();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(_options.EnvironmentName))
        {
            builder.UseEnvironment(_options.EnvironmentName);
        }

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var redisAllowMissing = string.Equals(_redisConnectionString, "CHANGE_ME", StringComparison.Ordinal);
            var values = new Dictionary<string, string?>
            {
                ["App:Name"] = _options.AppName ?? "RitzauDemo",
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Redis:Configuration"] = _redisConnectionString,
                ["Redis:AllowMissing"] = redisAllowMissing.ToString(),
                ["Cache:Invalidation:NodeId"] = _cacheInvalidationNodeId,
                ["Cache:Invalidation:ChannelName"] = _cacheInvalidationChannelName,
                ["Database:RunMigrationsAtStartup"] = _options.RunMigrationsAtStartup.ToString(),
                ["AuthorBooks:SeedAtStartup"] = _options.AuthorBooksSeedAtStartup.ToString(),
                ["DataProtection:KeyEncryptionCertificatePath"] = _keyCertificate.Path,
                ["DataProtection:KeyEncryptionCertificatePassword"] = TestKeyEncryptionCertificate.Password,
                ["ForwardedHeaders:KnownNetworks:0"] = "0.0.0.0/0",
                ["ForwardedHeaders:KnownNetworks:1"] = "::/0",
                ["RateLimiting:Global:PermitLimit"] = (_options.GlobalRateLimitPermitLimit ?? DefaultGlobalPermitLimit).ToString(),
                ["RateLimiting:Api:PermitLimit"] = (_options.ApiRateLimitPermitLimit ?? DefaultApiPermitLimit).ToString(),
                ["Observability:OpenTelemetry:Enabled"] = (_options.OpenTelemetryEnabled ?? false).ToString(),
                ["Observability:OpenTelemetry:Endpoint"] = _options.OpenTelemetryEndpoint ?? "http://127.0.0.1:4317",
                ["Cache:Invalidation:Enabled"] = (_options.CacheInvalidationEnabled ?? !string.Equals(_redisConnectionString, "CHANGE_ME", StringComparison.Ordinal)).ToString()
            };

            foreach (var (key, value) in _options.ConfigurationOverrides)
            {
                values[key] = value;
            }

            if (_options.LocalListTtlSeconds is not null)
            {
                values["Cache:Books:LocalListTtlSeconds"] = _options.LocalListTtlSeconds.Value.ToString();
            }

            if (_options.LocalItemTtlSeconds is not null)
            {
                values["Cache:Books:LocalItemTtlSeconds"] = _options.LocalItemTtlSeconds.Value.ToString();
            }

            if (_options.DisableLocalCache is not null)
            {
                values["Cache:Books:DisableLocalCache"] = _options.DisableLocalCache.Value.ToString();
            }

            configuration.AddInMemoryCollection(values);
        });

        builder.ConfigureTestServices(services =>
        {
            if (!_options.UseRealIdentityCookies)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = _options.UseIdentityCookieChallenge
                        ? IdentityConstants.ApplicationScheme
                        : TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
            }

            _options.ConfigureTestServices?.Invoke(services);
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException(
                "Database respawner was not initialized. Set WebAppFactoryOptions.InitializeDatabaseRespawner to true before using ResetDatabaseAsync.");
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);

        using var scope = Services.CreateScope();
        var cache = scope.ServiceProvider.GetService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        if (cache is not null)
        {
            try { await cache.RemoveByTagAsync(BooksCacheKeys.AllTag); } catch { }
            try { await cache.RemoveByTagAsync(AuthorBooksCacheKeys.AllTag); } catch { }
        }
    }

    public HttpClient CreateAuthenticatedClient(string userName = "integration-user@example.test", params string[] roles)
    {
        if (_options.UseRealIdentityCookies)
        {
            throw new InvalidOperationException("Real-cookie fixtures must sign in through Identity; fake principal headers are disabled.");
        }

        var client = CreateDefaultClient(
            ClientOptions.BaseAddress,
            new RedirectHandler(ClientOptions.MaxAutomaticRedirections),
            new TestAccountHeadersHandler(),
            new CookieContainerHandler());
        client.Timeout = TimeSpan.FromMinutes(MaxWaitTimeMinutes);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserHeader, userName);
        if (roles.Length > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RolesHeader, string.Join(",", roles));
        }

        return client;
    }

    public async ValueTask InitializeAsync()
    {
        if (_dbContainer is not null)
        {
            await _dbContainer.StartAsync();
            _connectionString = _dbContainer.GetConnectionString();
        }
        else
        {
            _connectionString = _options.PostgresConnectionString!;
        }

        if (_redisContainer is not null)
        {
            await _redisContainer.StartAsync();
            _redisConnectionString = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";
        }
        else
        {
            _redisConnectionString = string.IsNullOrWhiteSpace(_options.RedisConnectionString)
                ? "CHANGE_ME"
                : _options.RedisConnectionString;
        }

        if (_options.UseProcessEnvironmentOverrides)
        {
            // Minimal hosting reads some configuration before WebApplicationFactory
            // configuration callbacks can replace it. Keep these overrides scoped
            // and test parallelization disabled while this startup-time coupling exists.
            var redisAllowMissing = string.Equals(_redisConnectionString, "CHANGE_ME", StringComparison.Ordinal);
            var environmentValues = new Dictionary<string, string?>
            {
                [AppNameEnvironmentVariable] = _options.AppName ?? "RitzauDemo",
                [ConnectionStringEnvironmentVariable] = _connectionString,
                [RedisConfigurationEnvironmentVariable] = _redisConnectionString,
                [RedisAllowMissingEnvironmentVariable] = redisAllowMissing.ToString(),
                [CacheInvalidationEnabledEnvironmentVariable] = (_options.CacheInvalidationEnabled ?? !string.Equals(_redisConnectionString, "CHANGE_ME", StringComparison.Ordinal)).ToString(),
                [CacheInvalidationNodeIdEnvironmentVariable] = _cacheInvalidationNodeId,
                [CacheInvalidationChannelEnvironmentVariable] = _cacheInvalidationChannelName,
                [CacheBooksLocalListTtlEnvironmentVariable] = _options.LocalListTtlSeconds?.ToString(),
                [CacheBooksLocalItemTtlEnvironmentVariable] = _options.LocalItemTtlSeconds?.ToString(),
                [CacheBooksDisableLocalEnvironmentVariable] = _options.DisableLocalCache?.ToString(),
                [StartupMigrationsEnvironmentVariable] = _options.RunMigrationsAtStartup.ToString(),
                [AuthorBooksSeedAtStartupEnvironmentVariable] = _options.AuthorBooksSeedAtStartup.ToString(),
                ["DataProtection__KeyEncryptionCertificatePath"] = _keyCertificate.Path,
                ["DataProtection__KeyEncryptionCertificatePassword"] = TestKeyEncryptionCertificate.Password,
                [ForwardedHeaderKnownNetworkV4EnvironmentVariable] = "0.0.0.0/0",
                [ForwardedHeaderKnownNetworkV6EnvironmentVariable] = "::/0",
                [GlobalRateLimitEnvironmentVariable] = (_options.GlobalRateLimitPermitLimit ?? DefaultGlobalPermitLimit).ToString(),
                [ApiRateLimitEnvironmentVariable] = (_options.ApiRateLimitPermitLimit ?? DefaultApiPermitLimit).ToString(),
                [OpenTelemetryEnabledEnvironmentVariable] = (_options.OpenTelemetryEnabled ?? false).ToString(),
                [OpenTelemetryEndpointEnvironmentVariable] = _options.OpenTelemetryEndpoint ?? "http://127.0.0.1:4317"
            };
            // Account composition is selected before the minimal-host configuration
            // callbacks. Apply only this fixture's explicit overrides to that phase too.
            foreach (var (key, value) in _options.ConfigurationOverrides)
            {
                environmentValues[key.Replace(":", "__", StringComparison.Ordinal)] = value;
            }
            _environmentOverrides = new EnvironmentVariableScope(environmentValues);
        }

        HttpClient = CreateClient();
        HttpClient.Timeout = TimeSpan.FromMinutes(MaxWaitTimeMinutes);

        if (_options.InitializeDatabaseRespawner)
        {
            await InitializeRespawner();
        }
    }

    private async Task InitializeRespawner()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore = [new Respawn.Graph.Table("__EFMigrationsHistory")]
            });
    }

    public new async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        List<Exception> failures = [];
        try
        {
            HttpClient?.Dispose();
            await base.DisposeAsync();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }

        try
        {
            if (_redisContainer is not null)
            {
                await _redisContainer.DisposeAsync();
            }
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }

        try
        {
            // DisposeAsync removes the container; StopAsync left it on the runner.
            if (_dbContainer is not null)
            {
                await _dbContainer.DisposeAsync();
            }
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
        finally
        {
            try
            {
                _environmentOverrides?.Dispose();
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }

            _environmentOverrides = null;
            _keyCertificate.Dispose();
        }

        if (failures.Count == 1)
        {
            ExceptionDispatchInfo.Capture(failures[0]).Throw();
        }

        if (failures.Count > 1)
        {
            throw new AggregateException("One or more integration resources failed to dispose.", failures);
        }
    }
}
