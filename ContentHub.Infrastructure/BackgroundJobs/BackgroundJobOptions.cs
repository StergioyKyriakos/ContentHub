namespace ContentHub.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobOptions
{
    public const string SectionName = "BackgroundJobs";

    public bool Enabled { get; set; } = true;

    public bool RequireDistributedLock { get; set; }

    public ScheduledPostPublisherOptions ScheduledPostPublisher { get; set; } = new();

    public NotificationDeliveryOptions NotificationDelivery { get; set; } = new();

    public ExpiredTokenCleanupOptions ExpiredTokenCleanup { get; set; } = new();
}

public sealed class ScheduledPostPublisherOptions
{
    public bool Enabled { get; set; } = true;

    public int IntervalSeconds { get; set; } = 60;

    public int BatchSize { get; set; } = 25;
}

public sealed class NotificationDeliveryOptions
{
    public bool Enabled { get; set; } = true;

    public int IntervalSeconds { get; set; } = 30;

    public int BatchSize { get; set; } = 50;
}

public sealed class ExpiredTokenCleanupOptions
{
    public bool Enabled { get; set; } = true;

    public int IntervalMinutes { get; set; } = 60;

    public int RetentionDays { get; set; } = 7;

    public int BatchSize { get; set; } = 100;
}
