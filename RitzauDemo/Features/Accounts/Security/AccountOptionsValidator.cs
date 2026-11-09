using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace RitzauDemo.Features.Accounts.Security;

internal sealed class AccountOptionsValidator(string appName) : IValidateOptions<AccountOptions>
{
    public ValidateOptionsResult Validate(string? name, AccountOptions options)
    {
        if (options.AdminSeed is null || options.UserSeed is null || options.WriteLimit is null)
        {
            return ValidateOptionsResult.Fail("Accounts:AdminSeed, Accounts:UserSeed and Accounts:WriteLimit must be configuration objects.");
        }

        List<string> errors = [];
        ValidateSeed(options.AdminSeed, "Accounts:AdminSeed", errors);
        ValidateSeed(options.UserSeed, "Accounts:UserSeed", errors);
        if (options.AdminSeed.Enabled && options.UserSeed.Enabled
            && string.Equals(options.AdminSeed.Email, options.UserSeed.Email, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Accounts:AdminSeed:Email and Accounts:UserSeed:Email must be different.");
        }
        if (options.WriteLimit.PermitLimit <= 0)
        {
            errors.Add("Accounts:WriteLimit:PermitLimit must be positive.");
        }
        if (options.WriteLimit.WindowSeconds <= 0)
        {
            errors.Add("Accounts:WriteLimit:WindowSeconds must be positive.");
        }
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private void ValidateSeed(AccountSeedOptions seed, string section, List<string> errors)
    {
        if (string.IsNullOrEmpty(seed.Email) != string.IsNullOrEmpty(seed.Password))
        {
            errors.Add($"{section}:Email and {section}:Password must be supplied together.");
        }
        if ((!string.IsNullOrEmpty(seed.Email) || seed.Enabled) && !IsEmail(seed.Email))
        {
            errors.Add($"{section}:Email must be a valid mailbox.");
        }
        if (seed.Enabled && string.IsNullOrEmpty(seed.Password))
        {
            errors.Add($"{section}:Password is required.");
        }
        if (seed.Enabled && !AccountPasswordValidator.ValidateCandidate(seed.Password, seed.Email, appName).Succeeded)
        {
            errors.Add($"{section}:Password must satisfy the account password policy.");
        }
    }

    private static bool IsEmail(string? value) => value is { Length: > 0 and <= 254 }
        && !value.Any(char.IsControl) && value == value.Trim() && new EmailAddressAttribute().IsValid(value);
}
