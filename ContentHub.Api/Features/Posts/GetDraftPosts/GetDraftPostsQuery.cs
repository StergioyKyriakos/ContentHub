namespace ContentHub.Api.Features.Posts.GetDraftPosts;

public sealed class GetDraftPostsQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}