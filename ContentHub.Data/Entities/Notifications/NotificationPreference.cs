using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Notifications;

public sealed class NotificationPreference : Entity
{
    public NotificationPreference()
    {
    }

    public NotificationPreference(
        Guid userId,
        NotificationType type,
        NotificationChannel channel,
        bool isEnabled)
    {
        UserId = userId;
        Type = type;
        Channel = channel;
        IsEnabled = isEnabled;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public NotificationChannel Channel { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public void Update(bool isEnabled)
    {
        IsEnabled = isEnabled;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");

        builder.HasKey(preference => preference.Id);

        builder.Property(preference => preference.Id)
            .ValueGeneratedNever();

        builder.Property(preference => preference.UserId)
            .IsRequired();

        builder.Property(preference => preference.Type)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(preference => preference.Channel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(preference => preference.IsEnabled)
            .IsRequired();

        builder.Property(preference => preference.CreatedAtUtc)
            .IsRequired();

        builder.Property(preference => preference.UpdatedAtUtc);

        builder.HasIndex(preference => preference.UserId);

        builder.HasIndex(preference => new
        {
            preference.UserId,
            preference.Type,
            preference.Channel
        }).IsUnique();
    }
}