namespace ContentHub.Api.Features.Notifications.Shared;

public static class NotificationEndpoints
{
    public const string GetAll = "/api/notifications";

    public const string MarkAsRead = "/api/notifications/{id:guid}/read";

    public const string MarkAllAsRead = "/api/notifications/read-all";

    public const string GetPreferences = "/api/notifications/preferences";

    public const string UpdatePreferences = "/api/notifications/preferences";
}