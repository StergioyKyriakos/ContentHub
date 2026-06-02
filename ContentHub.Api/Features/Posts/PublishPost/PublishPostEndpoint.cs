using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Entities.Notifications;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.PublishPost;

public sealed class PublishPostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(PostEndpoints.Publish, Handle)
            .WithTags("Posts")
            .WithName("PublishPost")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [FromBody] PublishPostCommand command,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        var post = await db.Posts
            .Include(post => post.Categories)
            .Include(post => post.Authors)
            .ThenInclude(postAuthor => postAuthor.Author)
            .FirstOrDefaultAsync(post => post.Id == command.Id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        if (post.IsArchived)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.CannotPublishArchived));
        }

        if (string.IsNullOrWhiteSpace(post.Title) ||
            string.IsNullOrWhiteSpace(post.Slug) ||
            string.IsNullOrWhiteSpace(post.Content))
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(
                ApiError.Create(
                    code: "posts.missing_required_fields",
                    message: "A post needs title, slug and content before it can be published.")));
        }

        if (post.Categories.Count == 0)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.CategoryRequired));
        }

        if (post.Authors.Count == 0)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.AuthorRequired));
        }
        
        var oldValues = new
        {
            post.Status,
            post.PublishedAtUtc
        };

        post.Publish();
        
        var authorUserIds = post.Authors
            .Select(postAuthor => postAuthor.Author.UserId)
            .Where(userId => userId.HasValue)
            .Select(userId => userId!.Value)
            .Distinct()
            .ToList();

        if (authorUserIds.Count == 0)
        {
            authorUserIds.Add(post.CreatedById);
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
                message: $"Your post \"{post.Title}\" has been published.");

            db.Notifications.Add(notification);

            db.NotificationDeliveries.Add(new NotificationDelivery(
                notificationId: notification.Id,
                channel: NotificationChannel.InApp));
        }
        
        auditLogWriter.Add(
            action: AuditAction.PostPublished,
            entityName: "Post",
            entityId: post.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                post.Status,
                post.PublishedAtUtc
            });
        
        await db.SaveChangesAsync(ct);

        var response = new PublishPostResponse
        {
            Id = post.Id,
            Status = PostStatusDto.Published,
            PublishedAtUtc = post.PublishedAtUtc
        };

        return Results.Ok(ApiResponse<PublishPostResponse>.Ok(response));
    }
}
