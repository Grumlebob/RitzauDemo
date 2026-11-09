namespace RitzauDemo.Test.E2E.Support;

internal sealed record SeededE2EAccount(string Email, string Password);

internal static class SeededE2EAccounts
{
    public static SeededE2EAccount ResolveAdmin() => Resolve("Admin");

    public static SeededE2EAccount ResolveUser() => Resolve("User");

    private static SeededE2EAccount Resolve(string role)
    {
        var environmentPrefix = $"E2E_{role.ToUpperInvariant()}";
        var email = Environment.GetEnvironmentVariable($"{environmentPrefix}_EMAIL");
        var password = Environment.GetEnvironmentVariable($"{environmentPrefix}_PASSWORD");
        if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(password))
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                return new SeededE2EAccount(email, password);
            }

            throw new InvalidOperationException($"Set both {environmentPrefix}_EMAIL and {environmentPrefix}_PASSWORD for the seeded {role}.");
        }

        var values = ReadRepositoryDotEnv();
        var prefix = $"Accounts__{role}Seed";
        if (values.TryGetValue($"{prefix}__Email", out email) && !string.IsNullOrEmpty(email) &&
            values.TryGetValue($"{prefix}__Password", out password) && !string.IsNullOrEmpty(password))
        {
            return new SeededE2EAccount(email, password);
        }

        throw new InvalidOperationException(
            $"The browser test needs an enabled seeded {role}. Configure {prefix}:Email and {prefix}:Password in .env, or set the E2E_{role.ToUpperInvariant()}_EMAIL and E2E_{role.ToUpperInvariant()}_PASSWORD environment variables.");
    }

    // Read KEY=value without trimming or unquoting; Compose uses format: raw for this file.
    private static Dictionary<string, string> ReadRepositoryDotEnv()
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var root = FindRepositoryRoot();
        if (root is null)
        {
            return values;
        }

        var envPath = Path.Combine(root, ".env");
        if (!File.Exists(envPath))
        {
            return values;
        }

        foreach (var line in File.ReadLines(envPath))
        {
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf('=', StringComparison.Ordinal);
            if (separator > 0)
            {
                values[line[..separator]] = line[(separator + 1)..];
            }
        }

        return values;
    }

    private static string? FindRepositoryRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current is not null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "RitzauDemo.sln")))
            {
                return current.FullName;
            }
        }

        return null;
    }
}
