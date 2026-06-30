using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.Type)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.PayloadJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.OccurredAtUtc)
            .IsRequired();

        builder.Property(message => message.ProcessedAtUtc);

        builder.Property(message => message.FailedAtUtc);

        builder.Property(message => message.NextAttemptAtUtc);

        builder.Property(message => message.RetryCount)
            .IsRequired();

        builder.Property(message => message.Error)
            .HasMaxLength(4000);

        builder.HasIndex(message => message.Type);

        builder.HasIndex(message => message.OccurredAtUtc);

        builder.HasIndex(message => message.ProcessedAtUtc);

        builder.HasIndex(message => message.NextAttemptAtUtc);
    }
}
