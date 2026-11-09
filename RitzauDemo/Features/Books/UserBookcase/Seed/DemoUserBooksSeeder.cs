using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RitzauDemo.Core.Features.Books.Domain;
using RitzauDemo.Features.Accounts;
using RitzauDemo.Features.Books.Caching;
using ApplicationUser = RitzauDemo.Features.Accounts.Identity.ApplicationUser;

namespace RitzauDemo.Features.Books.UserBookcase.Seed;

internal sealed class DemoUserBooksSeeder(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<ApplicationUser> users,
    IOptions<AccountOptions> options,
    BooksCachePolicy cachePolicy,
    ILogger<DemoUserBooksSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seed = options.Value.UserSeed;
        if (!seed.Enabled)
        {
            return;
        }

        var user = await users.FindByEmailAsync(seed.Email);
        if (user is null)
        {
            throw new InvalidOperationException("The configured User seed account was not found after account seeding.");
        }

        List<int> seededBookIds = [];
        await using var strategyContext = await dbFactory.CreateDbContextAsync(cancellationToken);
        var strategy = strategyContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            seededBookIds.Clear();
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(73529301015422)", cancellationToken);

            if (await db.UserBooks.AnyAsync(book => book.OwnerUserId == user.Id, cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            Book[] books =
            [
                new()
                {
                    Title = "RitzauDemo Product Notes",
                    Author = "Demo Reader",
                    Url = "https://example.test/notes/product"
                },
                new()
                {
                    Title = "A Short Guide to Books",
                    Author = "Demo Reader",
                    Url = "https://example.test/notes/books"
                }
            ];

            foreach (var book in books)
            {
                db.Books.Add(book);
                db.UserBooks.Add(new UserBook { Book = book, OwnerUserId = user.Id });
            }

            await db.SaveChangesAsync(cancellationToken);
            seededBookIds.AddRange(books.Select(book => book.Id));
            await transaction.CommitAsync(cancellationToken);
        });

        foreach (var id in seededBookIds)
        {
            await cachePolicy.InvalidateBookAsync(user.Id, id);
        }

        if (seededBookIds.Count > 0)
        {
            logger.LogInformation("Seeded {BookCount} demo books for the configured User account.", seededBookIds.Count);
        }
    }
}

internal static class DemoUserBooksSeedExtensions
{
    public static async Task SeedDemoUserBooksAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DemoUserBooksSeeder>().SeedAsync();
    }
}
