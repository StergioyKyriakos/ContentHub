namespace ContentHub.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public bool Enabled { get; set; } = true;

    public bool RequireDistributedLock { get; set; }

    public int IntervalSeconds { get; set; } = 10;

    public int BatchSize { get; set; } = 50;

    public int RetryDelaySeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 5;
}
