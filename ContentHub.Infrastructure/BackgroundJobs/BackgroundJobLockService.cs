using ContentHub.Infrastructure.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ContentHub.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobLockService
{
    private const string ReleaseScript =
        "if redis.call('GET', KEYS[1]) == ARGV[1] then return redis.call('DEL', KEYS[1]) else return 0 end";

    private readonly RedisConnectionFactory _connectionFactory;
    private readonly ILogger<BackgroundJobLockService> _logger;

    public BackgroundJobLockService(
        RedisConnectionFactory connectionFactory,
        ILogger<BackgroundJobLockService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<BackgroundJobLock?> TryAcquireAsync(
        string key,
        TimeSpan ttl,
        bool requireDistributedLock,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionFactory.GetConnectionAsync();
            if (connection is null)
            {
                if (requireDistributedLock)
                {
                    _logger.LogWarning(
                        "Background job lock {LockKey} was not acquired because Redis is unavailable.",
                        key);

                    return null;
                }

                return BackgroundJobLock.LocalOnly();
            }

            var token = Guid.CreateVersion7().ToString("N");
            var acquired = await connection.GetDatabase().StringSetAsync(
                key,
                token,
                ttl,
                When.NotExists);

            return acquired
                ? BackgroundJobLock.Distributed(connection, key, token)
                : null;
        }
        catch (Exception ex)
        {
            if (requireDistributedLock)
            {
                _logger.LogWarning(
                    ex,
                    "Background job lock {LockKey} was not acquired because Redis failed.",
                    key);

                return null;
            }

            _logger.LogWarning(
                ex,
                "Background job lock {LockKey} is falling back to local-only execution.",
                key);

            return BackgroundJobLock.LocalOnly();
        }
    }

    public sealed class BackgroundJobLock : IAsyncDisposable
    {
        private readonly IConnectionMultiplexer? _connection;
        private readonly string? _key;
        private readonly string? _token;

        private BackgroundJobLock(
            IConnectionMultiplexer? connection,
            string? key,
            string? token)
        {
            _connection = connection;
            _key = key;
            _token = token;
        }

        public static BackgroundJobLock LocalOnly()
        {
            return new BackgroundJobLock(null, null, null);
        }

        public static BackgroundJobLock Distributed(
            IConnectionMultiplexer connection,
            string key,
            string token)
        {
            return new BackgroundJobLock(connection, key, token);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is null ||
                string.IsNullOrWhiteSpace(_key) ||
                string.IsNullOrWhiteSpace(_token))
            {
                return;
            }

            await _connection.GetDatabase().ScriptEvaluateAsync(
                ReleaseScript,
                [new RedisKey(_key)],
                [new RedisValue(_token)]);
        }
    }
}
