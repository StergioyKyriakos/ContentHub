using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ContentHub.Infrastructure.Caching;

public sealed class RedisConnectionFactory
{
    private readonly RedisOptions _options;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnectionMultiplexer? _connection;

    public RedisConnectionFactory(IOptions<RedisOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IConnectionMultiplexer?> GetConnectionAsync()
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            return null;
        }

        if (_connection is { IsConnected: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync();
        try
        {
            if (_connection is { IsConnected: true })
            {
                return _connection;
            }

            _connection?.Dispose();
            _connection = null;

            var configuration = ConfigurationOptions.Parse(_options.ConnectionString);
            configuration.AbortOnConnectFail = false;

            _connection = await ConnectionMultiplexer.ConnectAsync(configuration);

            return _connection;
        }
        catch
        {
            return null;
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}
