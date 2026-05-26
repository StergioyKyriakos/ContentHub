using ContentHub.Data.Dtos.Notifications;

namespace ContentHub.Api.Features.Notifications.GetNotificationPreferences;

public sealed class GetNotificationPreferencesResponse
{
    public IReadOnlyList<NotificationPreferenceDto> Preferences { get; set; } = [];
}