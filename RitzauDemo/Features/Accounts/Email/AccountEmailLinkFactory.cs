using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace RitzauDemo.Features.Accounts.Email;

internal sealed class AccountEmailLinkFactory(AccountOptions options, IAccountMailFrontend frontend)
{
    public AccountMailMessage Create(AccountMailPayload payload, string messageId)
    {
        var route = frontend.GetRoute(payload.Purpose);
        if (string.IsNullOrWhiteSpace(route) || route.StartsWith('/') || route.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The account mail frontend route is invalid.");
        }

        var baseUrl = $"{options.PublicOrigin.TrimEnd('/')}/{route}";
        var values = new Dictionary<string, string?>
        {
            ["userId"] = payload.UserId,
            ["code"] = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(payload.Token))
        };
        if (payload.TargetEmail is not null)
        {
            values["email"] = payload.TargetEmail;
        }

        var link = QueryHelpers.AddQueryString(baseUrl, values);
        var subject = payload.Purpose switch
        {
            AccountMailPurpose.Confirmation => "Confirm your account",
            AccountMailPurpose.PasswordReset => "Reset your password",
            AccountMailPurpose.EmailChange => "Confirm your new email",
            _ => throw new InvalidOperationException("Unsupported account mail purpose.")
        };
        var expiry = payload.LinkExpiresUtc.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var text = $"{subject}\n\nOpen this link before {expiry} UTC:\n{link}\n\nIf you did not request this, ignore this email.";
        return new AccountMailMessage(messageId, payload.Recipient, subject, text);
    }
}
