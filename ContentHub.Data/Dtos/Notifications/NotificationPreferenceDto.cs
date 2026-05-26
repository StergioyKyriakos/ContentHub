using ContentHub.Data.Enums;

namespace ContentHub.Data.Dtos.Notifications;

public sealed class NotificationPreferenceDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public string TypeName { get; set; } = null!;

    public NotificationChannel Channel { get; set; }

    public string ChannelName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}

