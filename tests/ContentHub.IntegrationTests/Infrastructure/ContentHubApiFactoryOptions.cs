namespace ContentHub.IntegrationTests.Infrastructure;

public sealed class ContentHubApiFactoryOptions
{
    public bool RedisEnabled { get; set; } = false;

    public bool BackgroundJobsEnabled { get; set; } = false;

    public bool OutboxEnabled { get; set; } = false;

    public int ScheduledPostPublisherIntervalSeconds { get; set; } = 60;

    public int NotificationDeliveryIntervalSeconds { get; set; } = 30;

    public int OutboxIntervalSeconds { get; set; } = 10;

    public int ExpiredTokenCleanupIntervalMinutes { get; set; } = 60;
}
