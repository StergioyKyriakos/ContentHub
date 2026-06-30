namespace ContentHub.Infrastructure.Outbox;

public sealed class PostPublishedOutboxPayload
{
    public Guid PostId { get; set; }

    public string PostTitle { get; set; } = string.Empty;

    public Guid CreatedById { get; set; }

    public List<Guid> AuthorUserIds { get; set; } = [];

    public Guid? ActorUserId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public PostPublishedAuditValues OldValues { get; set; } = new();

    public PostPublishedAuditValues NewValues { get; set; } = new();
}

public sealed class PostPublishedAuditValues
{
    public string? Status { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime? ScheduledForUtc { get; set; }
}
