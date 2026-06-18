using ContentHub.Application.Abstractions.Caching;

namespace ContentHub.Infrastructure.Caching;

public sealed class CacheInvalidationService
{
    private readonly ICacheService _cacheService;

    public CacheInvalidationService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task InvalidateFeaturedPostsAsync(CancellationToken cancellationToken = default)
    {
        var pageSizes = new[] { 10, 20, 50, 100 };

        for (var page = 1; page <= 10; page++)
        {
            foreach (var pageSize in pageSizes)
            {
                await _cacheService.RemoveAsync(
                    CacheKeys.PublicFeaturedPosts(page, pageSize),
                    cancellationToken);
            }
        }
    }
}
