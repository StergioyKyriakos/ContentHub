using ContentHub.Data.Dtos.Posts;

namespace ContentHub.Api.Features.Posts.SchedulePost;

public sealed class SchedulePostResponse
{
    public Guid Id { get; set; }
    public PostStatusDto Status { get; set; }
    public DateTime? ScheduledForUtc { get; set; }
}