using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.BackgroundJobs;

public sealed class ExpiredTokenCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<BackgroundJobOptions> _options;
    private readonly ILogger<ExpiredTokenCleanupJob> _logger;
    private readonly BackgroundJobLockService _lockService;

    public ExpiredTokenCleanupJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<BackgroundJobOptions> options,
        ILogger<ExpiredTokenCleanupJob> logger,
        BackgroundJobLockService lockService)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
        _lockService = lockService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessAsync(stoppingToken);
            await Task.Delay(GetInterval(), stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        var options = _options.CurrentValue;

        if (!options.Enabled || !options.ExpiredTokenCleanup.Enabled)
        {
            return;
        }

        try
        {
            var lease = await _lockService.TryAcquireAsync(
                "background-jobs:expired-token-cleanup",
                GetInterval().Add(TimeSpan.FromMinutes(1)),
                options.RequireDistributedLock,
                ct);

            if (lease is null)
            {
                return;
            }

            await using (lease)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<ExpiredTokenCleanupService>();

                await cleanupService.CleanupAsync(options.ExpiredTokenCleanup, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Expired token cleanup job failed.");
        }
    }

    private TimeSpan GetInterval()
    {
        var minutes = Math.Max(_options.CurrentValue.ExpiredTokenCleanup.IntervalMinutes, 5);

        return TimeSpan.FromMinutes(minutes);
    }
}
