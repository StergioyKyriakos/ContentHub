namespace ContentHub.Application.Abstractions.Caching;

public static class CacheKeys
{
    public const string PublicFeaturedPostsPrefix = "public:featured-posts";

    public static string PublicFeaturedPosts(
        int page,
        int pageSize)
    {
        return $"{PublicFeaturedPostsPrefix}:page:{page}:size:{pageSize}";
    }
}
