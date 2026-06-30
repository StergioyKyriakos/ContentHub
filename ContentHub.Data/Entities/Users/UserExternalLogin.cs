using System.Diagnostics.CodeAnalysis;
using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Users;

public sealed class UserExternalLogin : Entity
{
    public UserExternalLogin()
    {
    }

    [SetsRequiredMembers]
    public UserExternalLogin(
        Guid userId,
        string provider,
        string providerUserId,
        string? email,
        string? displayName)
    {
        UserId = userId;
        Provider = provider;
        ProviderUserId = providerUserId;
        Email = email;
        DisplayName = displayName;
    }

    public Guid UserId { get; set; }

    public required string Provider { get; set; }

    public required string ProviderUserId { get; set; }

    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

public sealed class UserExternalLoginConfiguration : IEntityTypeConfiguration<UserExternalLogin>
{
    public void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ToTable("user_external_logins");

        builder.HasKey(login => login.Id);

        builder.Property(login => login.Id)
            .ValueGeneratedNever();

        builder.Property(login => login.UserId)
            .IsRequired();

        builder.Property(login => login.Provider)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(login => login.ProviderUserId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(login => login.Email)
            .HasMaxLength(320);

        builder.Property(login => login.DisplayName)
            .HasMaxLength(150);

        builder.Property(login => login.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(login => login.UserId);

        builder.HasIndex(login => new
            {
                login.Provider,
                login.ProviderUserId
            })
            .IsUnique();
    }
}
