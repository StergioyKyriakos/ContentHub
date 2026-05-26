using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Notifications;

public sealed class Notification : Entity
{
    public Notification()
    {
    }

    public Notification(
        Guid userId,
        NotificationType type,
        string title,
        string message)
    {
        UserId = userId;
        Type = type;
        Title = title.Trim();
        Message = message.Trim();
        Status = NotificationStatus.Unread;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public NotificationStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ReadAtUtc { get; set; }

    public bool IsRead => Status == NotificationStatus.Read;

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Read)
        {
            return;
        }

        Status = NotificationStatus.Read;
        ReadAtUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = NotificationStatus.Archived;
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.Id)
            .ValueGeneratedNever();

        builder.Property(notification => notification.UserId)
            .IsRequired();

        builder.Property(notification => notification.Type)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(notification => notification.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(notification => notification.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(notification => notification.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(notification => notification.CreatedAtUtc)
            .IsRequired();

        builder.Property(notification => notification.ReadAtUtc);

        builder.HasIndex(notification => notification.UserId);

        builder.HasIndex(notification => notification.Status);

        builder.HasIndex(notification => notification.Type);

        builder.HasIndex(notification => notification.CreatedAtUtc);
    }
}