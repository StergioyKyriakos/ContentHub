using ContentHub.Data.Entities.Common;

namespace ContentHub.Data.Entities.Outbox;

public sealed class OutboxMessage : Entity
{
    public OutboxMessage()
    {
    }

    public OutboxMessage(
        string type,
        string payloadJson)
    {
        Type = type.Trim();
        PayloadJson = payloadJson;
        OccurredAtUtc = DateTime.UtcNow;
        NextAttemptAtUtc = DateTime.UtcNow;
    }

    public string Type { get; set; } = null!;

    public string PayloadJson { get; set; } = null!;

    public DateTime OccurredAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    public DateTime? FailedAtUtc { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }

    public int RetryCount { get; set; }

    public string? Error { get; set; }

    public void MarkProcessed()
    {
        ProcessedAtUtc = DateTime.UtcNow;
        FailedAtUtc = null;
        NextAttemptAtUtc = null;
        Error = null;
    }

    public void MarkFailed(
        string error,
        int retryDelaySeconds)
    {
        RetryCount++;
        FailedAtUtc = DateTime.UtcNow;
        NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(Math.Max(retryDelaySeconds, 1));
        Error = error.Length <= 4000 ? error : error[..4000];
    }
}
