using System.Diagnostics.CodeAnalysis;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Users;

public sealed class User : AggregateRoot
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<UserSession> _sessions = [];

    public User()
    {
    }
    [SetsRequiredMembers]
    public User(
        string email,
        string username,
        string displayName,
        string passwordHash)
    {
        Email = email.Trim();
        NormalizedEmail = email.Trim().ToUpperInvariant();
        Username = username.Trim();
        NormalizedUsername = username.Trim().ToUpperInvariant();
        DisplayName = displayName.Trim();
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
        EmailVerified = false;
    }

    public required string Email { get; set; } 

    public required string NormalizedEmail { get; set; } 

    public required string Username { get; set; } 
    public required string DisplayName { get; set; } 


    public required string NormalizedUsername { get; set; } 

    public required string PasswordHash { get; set; }

    public UserStatus Status { get;  set; }

    public bool EmailVerified { get;  set; }

    public DateTime? LastLoginAtUtc { get;  set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public IReadOnlyCollection<UserSession> Sessions => _sessions.AsReadOnly();

    public bool IsActive => Status == UserStatus.Active;

    public void MarkEmailAsVerified()
    {
        EmailVerified = true;
        MarkAsUpdated();
    }

    public void MarkLoggedIn()
    {
        LastLoginAtUtc = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Disable()
    {
        Status = UserStatus.Disabled;
        MarkAsUpdated();
    }

    public void Enable()
    {
        Status = UserStatus.Active;
        MarkAsUpdated();
    }

    public UserRole AddRole(Guid roleId)
    {
        var userRole = new UserRole(Id, roleId);

        _userRoles.Add(userRole);

        return userRole;
    }

    public RefreshToken AddRefreshToken(
        string tokenHash,
        DateTime expiresAtUtc,
        string? userAgent,
        string? ipAddress)
    {
        var refreshToken = new RefreshToken(
            userId: Id,
            tokenHash: tokenHash,
            expiresAtUtc: expiresAtUtc,
            userAgent: userAgent,
            ipAddress: ipAddress);

        _refreshTokens.Add(refreshToken);

        return refreshToken;
    }

    public UserSession AddSession(
        string refreshTokenHash,
        DateTime expiresAtUtc,
        string? userAgent,
        string? ipAddress)
    {
        var session = new UserSession(
            userId: Id,
            refreshTokenHash: refreshTokenHash,
            expiresAtUtc: expiresAtUtc,
            userAgent: userAgent,
            ipAddress: ipAddress);

        _sessions.Add(session);

        return session;
    }
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.Username)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(user => user.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.NormalizedUsername)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(user => user.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(user => user.EmailVerified)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();

        builder.Property(user => user.UpdatedAtUtc);

        builder.Property(user => user.DeletedAtUtc);

        builder.Property(user => user.IsDeleted)
            .IsRequired();

        builder.Property(user => user.LastLoginAtUtc);

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();

        builder.HasIndex(user => user.NormalizedUsername)
            .IsUnique();

        builder.HasMany(user => user.RefreshTokens)
            .WithOne(refreshToken => refreshToken.User)
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Sessions)
            .WithOne(session => session.User)
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}