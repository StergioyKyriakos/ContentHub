using System.Diagnostics.CodeAnalysis;
using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Users;

public sealed class Role : Entity
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<Permission> _permissions = [];

    public Role() { }

    [SetsRequiredMembers]
    public Role(
        string name,
        string? description)
    {
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description?.Trim();
    }

    public required string Name { get; set; } 
    public required string NormalizedName { get; set; }
    public string? Description { get; set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .ValueGeneratedNever();

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(role => role.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(role => role.Description)
            .HasMaxLength(500);

        builder.HasIndex(role => role.NormalizedName)
            .IsUnique();

        builder.HasMany(role => role.Permissions)
            .WithOne(permission => permission.Role)
            .HasForeignKey(permission => permission.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}