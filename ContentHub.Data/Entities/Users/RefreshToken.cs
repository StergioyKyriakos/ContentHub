using System.Diagnostics.CodeAnalysis;
using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Users;

public sealed class RefreshToken : Entity
{
    public RefreshToken() { }

    [SetsRequiredMembers]
    public RefreshToken(
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

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsActive => !IsExpired && !IsRevoked;

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.Id)
            .ValueGeneratedNever();

        builder.Property(refreshToken => refreshToken.UserId)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.TokenHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.ExpiresAtUtc)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedAtUtc)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.RevokedAtUtc);

        builder.Property(refreshToken => refreshToken.ReplacedByTokenHash)
            .HasMaxLength(500);

        builder.Property(refreshToken => refreshToken.UserAgent)
            .HasMaxLength(500);

        builder.Property(refreshToken => refreshToken.IpAddress)
            .HasMaxLength(100);

        builder.HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        builder.HasIndex(refreshToken => refreshToken.UserId);
    }
}