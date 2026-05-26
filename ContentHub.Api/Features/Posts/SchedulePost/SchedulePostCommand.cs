namespace ContentHub.Api.Features.Posts.SchedulePost;

public sealed class SchedulePostCommand
{
    public Guid Id { get; set; }
    public DateTime ScheduledForUtc { get; set; }
}