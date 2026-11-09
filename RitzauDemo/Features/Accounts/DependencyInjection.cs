using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RitzauDemo.Features.Accounts.Security;
using RitzauDemo.Features.Accounts.Session;
using ApplicationUser = RitzauDemo.Features.Accounts.Identity.ApplicationUser;

namespace RitzauDemo.Features.Accounts;

internal static class AccountsServiceCollectionExtensions
{
    public static IServiceCollection AddAccountFeature(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var settings = configuration.GetSection("Accounts").Get<AccountOptions>() ?? new();
        var validation = new AccountOptionsValidator(configuration["App:Name"] ?? "RitzauDemo").Validate("Accounts", settings);
        if (validation.Failed)
        {
            throw new OptionsValidationException("Accounts", typeof(AccountOptions), validation.Failures);
        }
        services.AddSingleton(settings);
        services.AddSingleton<IOptions<AccountOptions>>(Options.Create(settings));
        var identity = new AccountApplicationIdentity(configuration["App:Name"] ?? "RitzauDemo", environment.EnvironmentName);
        services.AddSingleton(identity);
        services.AddScoped<AccountOperations>();
        services.AddScoped<AccountMutationTransactions>();
        services.AddScoped<AccountRevocationHandler>();
        services.AddScoped<AccountSessionOperations>();
        services.TryAddSingleton(TimeProvider.System);

        services.RemoveAll<IPasswordValidator<ApplicationUser>>();
        services.AddScoped<IPasswordValidator<ApplicationUser>, AccountPasswordValidator>();

        services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            // The shared validator counts Unicode code points, not UTF-16 units.
            options.Password.RequiredLength = 1;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        });
        services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
        services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
        {
            options.Cookie.Name = $"__Host-Accounts-{identity.Id}";
            options.Cookie.Domain = null;
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = false;

            var defaultExpiry = options.ExpireTimeSpan;
            var existingSigningIn = options.Events.OnSigningIn;
            options.Events.OnSigningIn = async context =>
            {
                await existingSigningIn(context);
                if (context.Properties.IsPersistent
                    && context.Properties.IssuedUtc is { } issuedUtc
                    && context.Properties.ExpiresUtc == issuedUtc.Add(defaultExpiry))
                {
                    // The retained Blazor Identity forms set IsPersistent without an explicit expiry.
                    // Match the API's fixed 14-day remember-me ticket without extending explicit tickets.
                    context.Properties.ExpiresUtc = issuedUtc.AddDays(14);
                    context.Properties.AllowRefresh = false;
                }
            };
        });
        services.Configure<AntiforgeryOptions>(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = $"__Host-CSRF-{identity.Id}";
            options.Cookie.Domain = null;
            options.Cookie.Path = "/";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        return services;
    }
}
