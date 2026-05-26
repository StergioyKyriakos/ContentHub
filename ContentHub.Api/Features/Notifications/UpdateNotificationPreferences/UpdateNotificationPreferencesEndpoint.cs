using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Notifications.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Entities.Notifications;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Notifications.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut(NotificationEndpoints.UpdatePreferences, Handle)
            .WithTags("Notifications")
            .WithName("UpdateNotificationPreferences")
            .RequireAuthorization(Policies.AuthenticatedOnly)
            .AddEndpointFilter<ValidationFilter<UpdateNotificationPreferencesCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] UpdateNotificationPreferencesCommand request,
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

        var existingPreferences = await db.NotificationPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

        foreach (var item in request.Preferences)
        {
            var preference = existingPreferences.FirstOrDefault(p => 
                p.Type == item.Type && 
                p.Channel == item.Channel);

            if (preference is null)
            {
                db.NotificationPreferences.Add(new NotificationPreference(
                    userId: userId,
                    type: item.Type,
                    channel: item.Channel,
                    isEnabled: item.IsEnabled));
            }
            else
            {
                preference.Update(item.IsEnabled);
            }
        }

        await db.SaveChangesAsync(ct);

        var response = new UpdateNotificationPreferencesResponse();

        return Results.Ok(ApiResponse<UpdateNotificationPreferencesResponse>.Ok(response));
    }
}