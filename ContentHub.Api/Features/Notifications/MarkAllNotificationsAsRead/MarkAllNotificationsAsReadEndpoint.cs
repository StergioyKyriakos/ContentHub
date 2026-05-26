using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Notifications.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Notifications.MarkAllNotificationsAsRead;

public sealed class MarkAllNotificationsAsReadEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPatch(NotificationEndpoints.MarkAllAsRead, Handle)
            .WithTags("Notifications")
            .WithName("MarkAllNotificationsAsRead")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        ICurrentUserProvider currentUserProvider,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        if (currentUserProvider.UserId is null)
        {
            return Results.Json(
                ApiResponse<DomainError>.Fail(NotificationErrors.UserNotAuthenticated),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var notifications = await db.Notifications
            .Where(notification =>
                notification.UserId == currentUserProvider.UserId.Value &&
                notification.Status == NotificationStatus.Unread)
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await db.SaveChangesAsync(ct);

        var response = new MarkAllNotificationsAsReadResponse
        {
            Count = notifications.Count
        };

        return Results.Ok(ApiResponse<MarkAllNotificationsAsReadResponse>.Ok(response));
    }
}