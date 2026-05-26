using ContentHub.Data.Enums;

namespace ContentHub.Data.Dtos.Notifications;

public sealed class NotificationDeliveryDto
{
    public Guid Id { get; set; }

    public Guid NotificationId { get; set; }

    public NotificationChannel Channel { get; set; }

    public string ChannelName { get; set; } = null!;

    public NotificationStatus Status { get; set; }

    public string StatusName { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public string? ErrorMessage { get; set; }
}