using System.Diagnostics.CodeAnalysis;
using ContentHub.Data.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContentHub.Data.Entities.Users;

public sealed class Permission : Entity
{
    public Permission() { }

    [SetsRequiredMembers]
    public Permission(
        Guid roleId,
        string name,
        string? description)
    {
        RoleId = roleId;
        Name = name.Trim();
        Description = description?.Trim();
    }

    public Guid RoleId { get; set; }
    public required string Name { get; set; } 
    public string? Description { get; set; }

    public Role Role { get; set; } = null!;
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id)
            .ValueGeneratedNever();

        builder.Property(permission => permission.RoleId)
            .IsRequired();

        builder.Property(permission => permission.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(permission => permission.Description)
            .HasMaxLength(500);

        builder.HasIndex(permission => new
        {
            permission.RoleId,
            permission.Name
        }).IsUnique();
    }
}