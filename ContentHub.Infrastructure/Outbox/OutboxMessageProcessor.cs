using System.Text.Json;
using ContentHub.Data.Entities.AuditLogs;
using ContentHub.Data.Entities.Notifications;
using ContentHub.Data.Entities.Outbox;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.BackgroundJobs;
using ContentHub.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Outbox;

public sealed class OutboxMessageProcessor : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<OutboxOptions> _options;
    private readonly ILogger<OutboxMessageProcessor> _logger;
    private readonly BackgroundJobLockService _lockService;

    public OutboxMessageProcessor(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<OutboxOptions> options,
        ILogger<OutboxMessageProcessor> logger,
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
            if (_options.CurrentValue.Enabled)
            {
                await ProcessPendingAsync(stoppingToken);
            }

            await Task.Delay(GetInterval(), stoppingToken);
        }
    }

    public async Task<int> ProcessPendingAsync(CancellationToken ct = default)
    {
        var options = _options.CurrentValue;

        var lease = await _lockService.TryAcquireAsync(
            "outbox:processor",
            GetInterval().Add(TimeSpan.FromSeconds(30)),
            options.RequireDistributedLock,
            ct);

        if (lease is null)
        {
            return 0;
        }

        await using (lease)
        {
            return await ProcessBatchAsync(options, ct);
        }
    }

    private async Task<int> ProcessBatchAsync(
        OutboxOptions options,
        CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();
        var cacheInvalidationService = scope.ServiceProvider.GetRequiredService<CacheInvalidationService>();
        var now = DateTime.UtcNow;
        var batchSize = Math.Max(options.BatchSize, 1);
        var maxRetries = Math.Max(options.MaxRetries, 1);

        var messages = await db.OutboxMessages
            .Where(message =>
                message.ProcessedAtUtc == null &&
                message.RetryCount < maxRetries &&
                (message.NextAttemptAtUtc == null || message.NextAttemptAtUtc <= now))
            .OrderBy(message => message.OccurredAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

        var processedCount = 0;

        foreach (var message in messages)
        {
            try
            {
                await ProcessMessageAsync(db, cacheInvalidationService, message, ct);
                message.MarkProcessed();
                await db.SaveChangesAsync(ct);
                processedCount++;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message, options.RetryDelaySeconds);
                await db.SaveChangesAsync(ct);

                _logger.LogError(
                    ex,
                    "Outbox message {OutboxMessageId} failed.",
                    message.Id);
            }
        }

        return processedCount;
    }

    private static async Task ProcessMessageAsync(
        ContentHubDbContext db,
        CacheInvalidationService cacheInvalidationService,
        OutboxMessage message,
        CancellationToken ct)
    {
        if (message.Type == OutboxMessageTypes.PostPublished)
        {
            await ProcessPostPublishedAsync(db, cacheInvalidationService, message, ct);
            return;
        }

        throw new InvalidOperationException($"Unsupported outbox message type '{message.Type}'.");
    }

    private static async Task ProcessPostPublishedAsync(
        ContentHubDbContext db,
        CacheInvalidationService cacheInvalidationService,
        OutboxMessage message,
        CancellationToken ct)
    {
        var payload = JsonSerializer.Deserialize<PostPublishedOutboxPayload>(
            message.PayloadJson,
            JsonOptions);

        if (payload is null)
        {
            throw new InvalidOperationException("Post published outbox payload could not be read.");
        }

        db.AuditLogs.Add(new AuditLog(
            actorUserId: payload.ActorUserId,
            action: AuditAction.PostPublished,
            entityName: "Post",
            entityId: payload.PostId.ToString(),
            oldValuesJson: Serialize(payload.OldValues),
            newValuesJson: Serialize(payload.NewValues),
            ipAddress: payload.IpAddress,
            userAgent: payload.UserAgent));

        await AddNotificationsAsync(db, payload, ct);
        await cacheInvalidationService.InvalidateFeaturedPostsAsync(ct);
    }

    private static async Task AddNotificationsAsync(
        ContentHubDbContext db,
        PostPublishedOutboxPayload payload,
        CancellationToken ct)
    {
        var authorUserIds = payload.AuthorUserIds
            .DefaultIfEmpty(payload.CreatedById)
            .Distinct()
            .ToList();

        foreach (var authorUserId in authorUserIds)
        {
            var preferenceEnabled = await db.NotificationPreferences
                .AnyAsync(preference =>
                        preference.UserId == authorUserId &&
                        preference.Type == NotificationType.PostPublished &&
                        preference.Channel == NotificationChannel.InApp &&
                        preference.IsEnabled,
                    ct);

            var hasPreference = await db.NotificationPreferences
                .AnyAsync(preference =>
                        preference.UserId == authorUserId &&
                        preference.Type == NotificationType.PostPublished &&
                        preference.Channel == NotificationChannel.InApp,
                    ct);

            if (hasPreference && !preferenceEnabled)
            {
                continue;
            }

            var notification = new Notification(
                userId: authorUserId,
                type: NotificationType.PostPublished,
                title: "Post published",
                message: $"Your post \"{payload.PostTitle}\" has been published.");

            db.Notifications.Add(notification);

            db.NotificationDeliveries.Add(new NotificationDelivery(
                notificationId: notification.Id,
                channel: NotificationChannel.InApp));
        }
    }

    private static string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private TimeSpan GetInterval()
    {
        var seconds = Math.Max(_options.CurrentValue.IntervalSeconds, 1);

        return TimeSpan.FromSeconds(seconds);
    }
}
