using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Notifications.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Entities.Notifications;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Notifications;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Notifications.GetNotificationPreferences;

public sealed class GetNotificationPreferencesEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(NotificationEndpoints.GetPreferences, Handle)
            .WithTags("Notifications")
            .WithName("GetNotificationPreferences")
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

        var userId = currentUserProvider.UserId.Value;

        await EnsureDefaultPreferencesAsync(userId, db, ct);

        IQueryable<NotificationPreference> baseQuery = db.NotificationPreferences;

        var preferences = await baseQuery
            .AsNoTracking()
            .Where(preference => preference.UserId == userId)
            .OrderBy(preference => preference.Type)
            .Select(preference => new NotificationPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                Type = preference.Type,
                TypeName = preference.Type.ToString(),
                Channel = preference.Channel,
                ChannelName = preference.Channel.ToString(),
                IsEnabled = preference.IsEnabled,
                CreatedAtUtc = preference.CreatedAtUtc,
                UpdatedAtUtc = preference.UpdatedAtUtc
            })
            .ToListAsync(ct);

        var response = new GetNotificationPreferencesResponse
        {
            Preferences = preferences
        };

        return Results.Ok(ApiResponse<GetNotificationPreferencesResponse>.Ok(response));
    }

    private static async Task EnsureDefaultPreferencesAsync(
        Guid userId,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        IQueryable<NotificationPreference> baseQuery = db.NotificationPreferences;

        var existingTypes = await baseQuery
            .Where(preference =>
                preference.UserId == userId &&
                preference.Channel == NotificationChannel.InApp)
            .Select(preference => preference.Type)
            .ToListAsync(ct);

        var allTypes = Enum.GetValues<NotificationType>();
        var generatedAny = false;

        foreach (var type in allTypes)
        {
            if (existingTypes.Contains(type))
            {
                continue;
            }

            db.NotificationPreferences.Add(new NotificationPreference(
                userId: userId,
                type: type,
                channel: NotificationChannel.InApp,
                isEnabled: true));

            generatedAny = true;
        }

        if (generatedAny)
        {
            await db.SaveChangesAsync(ct);
        }
    }
}