using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.BackgroundJobs;

public sealed class NotificationDeliveryJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<BackgroundJobOptions> _options;
    private readonly ILogger<NotificationDeliveryJob> _logger;
    private readonly BackgroundJobLockService _lockService;

    public NotificationDeliveryJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<BackgroundJobOptions> options,
        ILogger<NotificationDeliveryJob> logger,
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

        if (!options.Enabled || !options.NotificationDelivery.Enabled)
        {
            return;
        }

        try
        {
            var lease = await _lockService.TryAcquireAsync(
                "background-jobs:notification-delivery",
                GetInterval().Add(TimeSpan.FromSeconds(30)),
                options.RequireDistributedLock,
                ct);

            if (lease is null)
            {
                return;
            }

            await using (lease)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();
                var batchSize = Math.Max(options.NotificationDelivery.BatchSize, 1);

                var deliveries = await db.NotificationDeliveries
                    .Where(delivery =>
                        delivery.Channel == NotificationChannel.InApp &&
                        delivery.DeliveredAtUtc == null &&
                        delivery.Status != NotificationStatus.Sent)
                    .OrderBy(delivery => delivery.CreatedAtUtc)
                    .Take(batchSize)
                    .ToListAsync(ct);

                if (deliveries.Count == 0)
                {
                    return;
                }

                foreach (var delivery in deliveries)
                {
                    delivery.MarkDelivered();
                }

                await db.SaveChangesAsync(ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification delivery job failed.");
        }
    }

    private TimeSpan GetInterval()
    {
        var seconds = Math.Max(_options.CurrentValue.NotificationDelivery.IntervalSeconds, 5);

        return TimeSpan.FromSeconds(seconds);
    }
}
