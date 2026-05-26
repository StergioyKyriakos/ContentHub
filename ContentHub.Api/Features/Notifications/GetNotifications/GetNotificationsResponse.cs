using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Notifications;

namespace ContentHub.Api.Features.Notifications.GetNotifications;

public sealed class GetNotificationsResponse
{
    public PagedResponse<NotificationDto> Notifications { get; set; } = null!;
}