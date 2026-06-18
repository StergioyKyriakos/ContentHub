namespace ContentHub.Api.Features.Posts.GetFeaturedPosts;

public sealed class GetFeaturedPostsQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10; 
}