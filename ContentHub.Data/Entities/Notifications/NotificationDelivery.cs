using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Notifications;

public sealed class NotificationDelivery : Entity
{
    public NotificationDelivery()
    {
    }

    public NotificationDelivery(
        Guid notificationId,
        NotificationChannel channel)
    {
        NotificationId = notificationId;
        Channel = channel;
        Status = NotificationStatus.Unread;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid NotificationId { get; set; }

    public NotificationChannel Channel { get; set; }

    public NotificationStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public string? ErrorMessage { get; set; }

    public Notification Notification { get; set; } = null!;

    public void MarkDelivered()
    {
        Status = NotificationStatus.Sent;
        DeliveredAtUtc = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
    }
}

public sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("notification_deliveries");

        builder.HasKey(delivery => delivery.Id);

        builder.Property(delivery => delivery.Id)
            .ValueGeneratedNever();

        builder.Property(delivery => delivery.NotificationId)
            .IsRequired();

        builder.Property(delivery => delivery.Channel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(delivery => delivery.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(delivery => delivery.CreatedAtUtc)
            .IsRequired();

        builder.Property(delivery => delivery.DeliveredAtUtc);

        builder.Property(delivery => delivery.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(delivery => delivery.NotificationId);

        builder.HasOne(delivery => delivery.Notification)
            .WithMany()
            .HasForeignKey(delivery => delivery.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
