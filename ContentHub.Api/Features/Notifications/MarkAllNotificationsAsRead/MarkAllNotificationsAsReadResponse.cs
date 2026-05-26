namespace ContentHub.Api.Features.Notifications.MarkAllNotificationsAsRead;

public sealed class MarkAllNotificationsAsReadResponse
{
    public string Message { get; set; } = "All notifications marked as read.";
    public int Count { get; set; }
}