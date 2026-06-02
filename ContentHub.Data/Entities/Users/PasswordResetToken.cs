using System.Diagnostics.CodeAnalysis;
using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Users;

public sealed class PasswordResetToken : Entity
{
    public PasswordResetToken() { }

    [SetsRequiredMembers]
    public PasswordResetToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? userAgent,
        string? ipAddress)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        UserAgent = userAgent;
        IpAddress = ipAddress;
    }

    public Guid UserId { get; set; }

    public required string TokenHash { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? ConsumedAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsConsumed => ConsumedAtUtc.HasValue;

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsActive => !IsExpired && !IsConsumed && !IsRevoked;

    public void Consume()
    {
        ConsumedAtUtc = DateTime.UtcNow;
    }

    public void Revoke()
    {
        RevokedAtUtc = DateTime.UtcNow;
    }
}

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.Id)
            .ValueGeneratedNever();

        builder.Property(token => token.UserId)
            .IsRequired();

        builder.Property(token => token.TokenHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(token => token.CreatedAtUtc)
            .IsRequired();

        builder.Property(token => token.ExpiresAtUtc)
            .IsRequired();

        builder.Property(token => token.ConsumedAtUtc);

        builder.Property(token => token.RevokedAtUtc);

        builder.Property(token => token.UserAgent)
            .HasMaxLength(500);

        builder.Property(token => token.IpAddress)
            .HasMaxLength(100);

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.HasIndex(token => token.UserId);

        builder.HasOne(token => token.User)
            .WithMany(user => user.PasswordResetTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
