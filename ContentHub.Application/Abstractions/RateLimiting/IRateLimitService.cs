namespace ContentHub.Application.Abstractions.RateLimiting;

public interface IRateLimitService
{
    Task<RateLimitResult> CheckAsync(
        string key,
        int maxAttempts,
        TimeSpan window,
        CancellationToken cancellationToken = default);

    Task<RateLimitResult> IncrementAsync(
        string key,
        int maxAttempts,
        TimeSpan window,
        CancellationToken cancellationToken = default);

    Task ResetAsync(
        string key,
        CancellationToken cancellationToken = default);
}

public sealed class RateLimitResult
{
    public bool IsLimited { get; init; }

    public int Attempts { get; init; }

    public int RemainingAttempts { get; init; }

    public TimeSpan RetryAfter { get; init; }
}
