namespace ContentHub.Api.Features.Posts.Shared;

public static class PostEndpoints
{
    public const string Create = "/api/posts";

    public const string Update = "/api/posts/{id:guid}";

    public const string Delete = "/api/posts/{id:guid}";

    public const string GetById = "/api/posts/{id:guid}";

    public const string GetAll = "/api/posts";

    public const string GetDrafts = "/api/posts/drafts";

    public const string Publish = "/api/posts/{id:guid}/publish";

    public const string Unpublish = "/api/posts/{id:guid}/unpublish";

    public const string Archive = "/api/posts/{id:guid}/archive";

    public const string Schedule = "/api/posts/{id:guid}/schedule";

    public const string SetFeatured = "/api/posts/{id:guid}/feature";

    public const string RemoveFeatured = "/api/posts/{id:guid}/feature";

    public const string PublicPosts = "/api/public/posts";

    public const string PublicPostBySlug = "/api/public/posts/{slug}";

    public const string PublicFeaturedPosts = "/api/public/featured-posts";
}