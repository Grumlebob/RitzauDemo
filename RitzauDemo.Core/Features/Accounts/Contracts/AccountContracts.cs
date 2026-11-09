namespace RitzauDemo.Core.Features.Accounts.Contracts;

public sealed record ClientConfiguration(int SchemaVersion, string ApplicationId);
public sealed record AccountProfile(string Id, string Email, IReadOnlyList<string> Roles);
public sealed record AccountSession(string CsrfToken, AccountProfile? User);
public sealed record AccountMessage(string Message);

public sealed class AccountLoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required(AllowEmptyStrings = true), StringLength(256, MinimumLength = 1)]
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class AccountPasswordRequest
{
    [Required(AllowEmptyStrings = true), StringLength(256, MinimumLength = 1)]
    public string Password { get; set; } = "";
}
