using ContentHub.Data.Entities.Assets;
using ContentHub.Data.Entities.AuditLogs;
using ContentHub.Data.Entities.Authors;
using ContentHub.Data.Entities.Categories;
using ContentHub.Data.Entities.Notifications;
using ContentHub.Data.Entities.Posts;
using ContentHub.Data.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Data.Persistence;

public sealed class ContentHubDbContext : DbContext
{
    public ContentHubDbContext(DbContextOptions<ContentHubDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostAuthor> PostAuthors => Set<PostAuthor>();
    public DbSet<PostAsset> PostAssets => Set<PostAsset>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetVersion> AssetVersions => Set<AssetVersion>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuditEntityChange> AuditEntityChanges => Set<AuditEntityChange>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentHubDbContext).Assembly);
    }
}