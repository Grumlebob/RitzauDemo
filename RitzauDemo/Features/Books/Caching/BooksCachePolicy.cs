using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using RitzauDemo.Infrastructure.Hosting.CacheInvalidation;

namespace RitzauDemo.Features.Books.Caching;

internal sealed class BooksCachePolicy(
    ICacheInvalidator cacheInvalidator,
    IOptions<BooksCacheOptions> cacheOptions,
    ILogger<BooksCachePolicy> logger)
{
    private readonly BooksCacheOptions _options = cacheOptions.Value ?? new BooksCacheOptions();

    public HybridCacheEntryOptions ListEntryOptions() => CreateEntryOptions(
        _options.ListTtlMinutes,
        _options.LocalListTtlSeconds);

    public HybridCacheEntryOptions ItemEntryOptions() => CreateEntryOptions(
        _options.ItemTtlMinutes,
        _options.LocalItemTtlSeconds);

    public async Task InvalidateBookAsync(string userId, int id)
    {
        try
        {
            var result = await cacheInvalidator.InvalidateAsync(
                BooksCacheKeys.ForChangedBook(userId, id),
                CancellationToken.None);
            foreach (var warning in result.Warnings)
            {
                logger.LogWarning("Books cache invalidation for book {BookId}: {CacheInvalidationWarning}", id, warning);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to invalidate Books cache for book {BookId}", id);
        }
    }

    private HybridCacheEntryOptions CreateEntryOptions(int distributedTtlMinutes, int localTtlSeconds)
    {
        var flags = _options.DisableLocalCache
            ? HybridCacheEntryFlags.DisableLocalCache
            : HybridCacheEntryFlags.None;

        return new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(Math.Max(1, distributedTtlMinutes)),
            LocalCacheExpiration = TimeSpan.FromSeconds(Math.Max(1, localTtlSeconds)),
            Flags = flags
        };
    }
}
