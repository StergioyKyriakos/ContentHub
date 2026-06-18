using ContentHub.Application.Abstractions.RateLimiting;
using ContentHub.Infrastructure.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.RateLimiting;

public sealed class RedisRateLimitService : IRateLimitService
{
    private readonly RedisConnectionFactory _connectionFactory;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RedisRateLimitService> _logger;

    public RedisRateLimitService(
        RedisConnectionFactory connectionFactory,
        IOptions<RateLimitOptions> options,
        ILogger<RedisRateLimitService> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RateLimitResult> CheckAsync(
        string key,
        int maxAttempts,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionFactory.GetConnectionAsync();
            if (connection is null)
            {
                return Unavailable(maxAttempts, window, null);
            }

            var db = connection.GetDatabase();
            var value = await db.StringGetAsync(key);
            var attempts = value.HasValue && int.TryParse(value.ToString(), out var parsed)
                ? parsed
                : 0;

            var ttl = await db.KeyTimeToLiveAsync(key);

            return BuildResult(attempts, maxAttempts, ttl ?? window);
        }
        catch (Exception ex)
        {
            return Unavailable(maxAttempts, window, ex);
        }
    }

    public async Task<RateLimitResult> IncrementAsync(
        string key,
        int maxAttempts,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionFactory.GetConnectionAsync();
            if (connection is null)
            {
                return Unavailable(maxAttempts, window, null);
            }

            var db = connection.GetDatabase();
            var attempts = (int)await db.StringIncrementAsync(key);

            if (attempts == 1)
            {
                await db.KeyExpireAsync(key, window);
            }

            var ttl = await db.KeyTimeToLiveAsync(key);

            return BuildResult(attempts, maxAttempts, ttl ?? window);
        }
        catch (Exception ex)
        {
            return Unavailable(maxAttempts, window, ex);
        }
    }

    public async Task ResetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionFactory.GetConnectionAsync();
            if (connection is null)
            {
                return;
            }

            await connection.GetDatabase().KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis rate limit reset failed for key {RateLimitKey}.", key);
        }
    }

    private static RateLimitResult BuildResult(
        int attempts,
        int maxAttempts,
        TimeSpan retryAfter)
    {
        var remaining = Math.Max(maxAttempts - attempts, 0);

        return new RateLimitResult
        {
            IsLimited = attempts >= maxAttempts,
            Attempts = attempts,
            RemainingAttempts = remaining,
            RetryAfter = retryAfter
        };
    }

    private static RateLimitResult NotLimited(int maxAttempts)
    {
        return new RateLimitResult
        {
            IsLimited = false,
            Attempts = 0,
            RemainingAttempts = maxAttempts,
            RetryAfter = TimeSpan.Zero
        };
    }

    private RateLimitResult Unavailable(
        int maxAttempts,
        TimeSpan window,
        Exception? exception)
    {
        if (_options.FailClosedWhenUnavailable)
        {
            _logger.LogWarning(
                exception,
                "Redis rate limiting is unavailable and configured to fail closed.");

            return BuildResult(maxAttempts, maxAttempts, window);
        }

        _logger.LogWarning(
            exception,
            "Redis rate limiting is unavailable and configured to fail open.");

        return NotLimited(maxAttempts);
    }
}
