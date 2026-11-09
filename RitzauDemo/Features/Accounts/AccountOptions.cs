namespace RitzauDemo.Features.Accounts;

// Bind once during composition. Routing, policies and workers share this snapshot.
internal sealed class AccountOptions
{
    public AccountSeedOptions AdminSeed { get; init; } = new();
    public AccountSeedOptions UserSeed { get; init; } = new();
    public AccountWriteLimitOptions WriteLimit { get; init; } = new();
}

internal sealed class AccountSeedOptions
{
    public bool Enabled { get; init; }
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";
}

internal sealed class AccountWriteLimitOptions
{
    public int PermitLimit { get; init; } = 20;
    public int WindowSeconds { get; init; } = 300;
}
