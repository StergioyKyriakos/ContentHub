using System.Text.Json;
using ContentHub.Application.Abstractions.Caching;

namespace ContentHub.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RedisConnectionFactory _connectionFactory;

    public RedisCacheService(RedisConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionFactory.GetConnectionAsync();
            if (connection is null)
            {
                return default;
            }

            var value = await connection.GetDatabase().StringGetAsync(key);

            return value.HasValue
                ? JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions)
                : default;
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionFactory.GetConnectionAsync();
            if (connection is null)
            {
                return;
            }

            var payload = JsonSerializer.Serialize(value, JsonOptions);

            await connection.GetDatabase().StringSetAsync(
                key,
                payload,
                absoluteExpirationRelativeToNow);
        }
        catch
        {
        }
    }

    public async Task RemoveAsync(
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
        catch
        {
        }
    }
}
