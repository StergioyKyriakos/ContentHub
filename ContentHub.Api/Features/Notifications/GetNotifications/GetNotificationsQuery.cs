using ContentHub.Data.Enums;

namespace ContentHub.Api.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQuery
{
    public NotificationStatus? Status { get; set; }

    public NotificationType? Type { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
