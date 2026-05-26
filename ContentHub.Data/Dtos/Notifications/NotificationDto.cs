using ContentHub.Data.Enums;

namespace ContentHub.Data.Dtos.Notifications;

public sealed class NotificationDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public string TypeName { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public NotificationStatus Status { get; set; }

    public string StatusName { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ReadAtUtc { get; set; }
}