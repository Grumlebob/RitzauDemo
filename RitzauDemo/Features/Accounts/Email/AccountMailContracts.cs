namespace RitzauDemo.Features.Accounts.Email;

internal enum AccountMailPurpose
{
    Confirmation,
    PasswordReset,
    EmailChange
}

internal sealed record AccountMailPayload(
    int Version,
    string Recipient,
    string UserId,
    AccountMailPurpose Purpose,
    string SecurityStamp,
    string Token,
    string? TargetEmail,
    DateTimeOffset LinkExpiresUtc);

internal sealed record AccountMailMessage(string MessageId, string Recipient, string Subject, string Text);

internal interface IAccountMailFrontend
{
    string GetRoute(AccountMailPurpose purpose);
}

internal sealed class AccountMailFrontend : IAccountMailFrontend
{
    public static AccountMailFrontend BlazorAuto { get; } = new();

    public string GetRoute(AccountMailPurpose purpose) => purpose switch
    {
        AccountMailPurpose.Confirmation => "Account/ConfirmEmail",
        AccountMailPurpose.PasswordReset => "Account/ResetPassword",
        AccountMailPurpose.EmailChange => "Account/ConfirmEmailChange",
        _ => throw new ArgumentOutOfRangeException(nameof(purpose))
    };
}

internal interface IAccountMailTransport
{
    Task SendAsync(AccountMailMessage message, CancellationToken cancellationToken);
}
