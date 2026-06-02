using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Notifications.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Entities.Notifications; 
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Notifications;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Notifications.GetNotifications;

public sealed class GetNotificationsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(NotificationEndpoints.GetAll, Handle)
            .WithTags("Notifications")
            .WithName("GetNotifications")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] GetNotificationsQuery query,
        IValidator<GetNotificationsQuery> validator,
        ICurrentUserProvider currentUserProvider,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        if (currentUserProvider.UserId is null)
        {
            return Results.Json(
                ApiResponse<object>.Fail(NotificationErrors.UserNotAuthenticated),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        IQueryable<Notification> notificationsQuery = db.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == currentUserProvider.UserId.Value);

        if (query.Status.HasValue)
        {
            notificationsQuery = notificationsQuery
                .Where(notification => notification.Status == query.Status.Value);
        }

        if (query.Type.HasValue)
        {
            notificationsQuery = notificationsQuery
                .Where(notification => notification.Type == query.Type.Value);
        }

        var totalItems = await notificationsQuery.CountAsync(ct);

        var items = await notificationsQuery
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(notification => new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                TypeName = notification.Type.ToString(),
                Title = notification.Title,
                Message = notification.Message,
                Status = notification.Status,
                StatusName = notification.Status.ToString(),
                CreatedAtUtc = notification.CreatedAtUtc,
                ReadAtUtc = notification.ReadAtUtc
            })
            .ToListAsync(ct);

        var response = new GetNotificationsResponse
        {
            Notifications = PagedResponse<NotificationDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems)
        };

        return Results.Ok(ApiResponse<GetNotificationsResponse>.Ok(response));
    }
}
