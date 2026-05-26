using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.Notifications.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommand
{
    public IReadOnlyCollection<NotificationPreferenceUpdateItem> Preferences { get; set; } = [];
}

public sealed class NotificationPreferenceUpdateItem
{
    public NotificationType Type { get; set; }

    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    public bool IsEnabled { get; set; }
}