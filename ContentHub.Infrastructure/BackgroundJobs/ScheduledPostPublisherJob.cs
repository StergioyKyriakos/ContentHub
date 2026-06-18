using System.Text.Json;
using ContentHub.Data.Entities.AuditLogs;
using ContentHub.Data.Entities.Notifications;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.BackgroundJobs;

public sealed class ScheduledPostPublisherJob : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<BackgroundJobOptions> _options;
    private readonly ILogger<ScheduledPostPublisherJob> _logger;
    private readonly BackgroundJobLockService _lockService;

    public ScheduledPostPublisherJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<BackgroundJobOptions> options,
        ILogger<ScheduledPostPublisherJob> logger,
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

        if (!options.Enabled || !options.ScheduledPostPublisher.Enabled)
        {
            return;
        }

        try
        {
            var lease = await _lockService.TryAcquireAsync(
                "background-jobs:scheduled-post-publisher",
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
                var cacheInvalidationService = scope.ServiceProvider.GetRequiredService<CacheInvalidationService>();
                var now = DateTime.UtcNow;
                var batchSize = Math.Max(options.ScheduledPostPublisher.BatchSize, 1);

                var posts = await db.Posts
                    .Include(post => post.Categories)
                    .Include(post => post.Authors)
                    .ThenInclude(postAuthor => postAuthor.Author)
                    .Where(post =>
                        post.Status == PostStatus.Scheduled &&
                        post.ScheduledForUtc != null &&
                        post.ScheduledForUtc <= now)
                    .OrderBy(post => post.ScheduledForUtc)
                    .Take(batchSize)
                    .ToListAsync(ct);

                if (posts.Count == 0)
                {
                    return;
                }

                foreach (var post in posts)
                {
                    if (!CanPublish(post))
                    {
                        _logger.LogWarning(
                            "Scheduled post {PostId} was skipped because it no longer meets publish requirements.",
                            post.Id);
                        continue;
                    }

                    var oldValues = new
                    {
                        post.Status,
                        post.PublishedAtUtc,
                        post.ScheduledForUtc
                    };

                    post.Publish();

                    db.AuditLogs.Add(new AuditLog(
                        actorUserId: null,
                        action: AuditAction.PostPublished,
                        entityName: "Post",
                        entityId: post.Id.ToString(),
                        oldValuesJson: Serialize(oldValues),
                        newValuesJson: Serialize(new
                        {
                            post.Status,
                            post.PublishedAtUtc,
                            post.ScheduledForUtc
                        }),
                        ipAddress: null,
                        userAgent: "ContentHub background job"));

                    await AddNotificationsAsync(db, post.Id, post.Title, post.CreatedById, ct);
                }

                await db.SaveChangesAsync(ct);
                await cacheInvalidationService.InvalidateFeaturedPostsAsync(ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled post publisher job failed.");
        }
    }

    private static async Task AddNotificationsAsync(
        ContentHubDbContext db,
        Guid postId,
        string postTitle,
        Guid createdById,
        CancellationToken ct)
    {
        var authorUserIds = await db.PostAuthors
            .Where(postAuthor => postAuthor.PostId == postId && postAuthor.Author.UserId != null)
            .Select(postAuthor => postAuthor.Author.UserId!.Value)
            .Distinct()
            .ToListAsync(ct);

        if (authorUserIds.Count == 0)
        {
            authorUserIds.Add(createdById);
        }

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
                message: $"Your post \"{postTitle}\" has been published.");

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

    private static bool CanPublish(Data.Entities.Posts.Post post)
    {
        return !string.IsNullOrWhiteSpace(post.Title) &&
               !string.IsNullOrWhiteSpace(post.Slug) &&
               !string.IsNullOrWhiteSpace(post.Content) &&
               post.Categories.Count > 0 &&
               post.Authors.Count > 0;
    }

    private TimeSpan GetInterval()
    {
        var seconds = Math.Max(_options.CurrentValue.ScheduledPostPublisher.IntervalSeconds, 10);

        return TimeSpan.FromSeconds(seconds);
    }
}
