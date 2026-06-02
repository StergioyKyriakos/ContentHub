using System.Diagnostics.CodeAnalysis;
using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Users;

public sealed class UserSession : Entity
{
    public UserSession() { }

    [SetsRequiredMembers]
    public UserSession(
        Guid userId,
        string refreshTokenHash,
        DateTime expiresAtUtc,
        string? userAgent,
        string? ipAddress)
    {
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        ExpiresAtUtc = expiresAtUtc;
        UserAgent = userAgent;
        IpAddress = ipAddress;
    }

    public Guid UserId { get; set; }
    public required string RefreshTokenHash { get; set; } 
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
    
    public User User { get; set; } = null!;

    public void Revoke()
    {
        RevokedAtUtc = DateTime.UtcNow;
    }

    public void RotateRefreshToken(
        string refreshTokenHash,
        DateTime expiresAtUtc,
        string? userAgent,
        string? ipAddress)
    {
        RefreshTokenHash = refreshTokenHash;
        ExpiresAtUtc = expiresAtUtc;
        UserAgent = userAgent;
        IpAddress = ipAddress;
    }
}

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(session => session.Id);

        builder.Property(session => session.Id)
            .ValueGeneratedNever();

        builder.Property(session => session.UserId)
            .IsRequired();

        builder.Property(session => session.RefreshTokenHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(session => session.CreatedAtUtc)
            .IsRequired();

        builder.Property(session => session.ExpiresAtUtc)
            .IsRequired();

        builder.Property(session => session.RevokedAtUtc);

        builder.Property(session => session.UserAgent)
            .HasMaxLength(500);

        builder.Property(session => session.IpAddress)
            .HasMaxLength(100);

        builder.HasIndex(session => session.UserId);

        builder.HasIndex(session => session.RefreshTokenHash);
    }
}
