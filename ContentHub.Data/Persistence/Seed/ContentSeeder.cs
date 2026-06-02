using ContentHub.Data.Entities.Authors;
using ContentHub.Data.Entities.Categories;
using ContentHub.Data.Entities.Posts;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Data.Persistence.Seed;

public sealed class ContentSeeder
{
    private const string AdminEmail = "admin@contenthub.local";

    public async Task SeedAsync(
        ContentHubDbContext db,
        CancellationToken cancellationToken = default)
    {
        var admin = await db.Users
            .FirstOrDefaultAsync(
                user => user.NormalizedEmail == AdminEmail.ToUpperInvariant(),
                cancellationToken);

        if (admin is null)
        {
            return;
        }

        var news = await EnsureCategoryAsync(
            db,
            name: "News",
            slug: "news",
            description: "Product announcements and editorial updates.",
            displayOrder: 10,
            cancellationToken);

        var guides = await EnsureCategoryAsync(
            db,
            name: "Guides",
            slug: "guides",
            description: "Practical publishing and content operations guides.",
            displayOrder: 20,
            cancellationToken);

        var releases = await EnsureCategoryAsync(
            db,
            name: "Releases",
            slug: "releases",
            description: "Release notes and platform changes.",
            displayOrder: 30,
            cancellationToken);

        var team = await EnsureAuthorAsync(
            db,
            displayName: "ContentHub Team",
            slug: "contenthub-team",
            bio: "The editorial team behind ContentHub.",
            userId: admin.Id,
            cancellationToken);

        await EnsurePostAsync(
            db,
            title: "Welcome to ContentHub",
            slug: "welcome-to-contenthub",
            summary: "A quick tour of the v1 publishing foundation.",
            content: "ContentHub v1 includes authentication, categories, authors, posts, assets, audit logs, search, notifications, and tests.",
            createdById: admin.Id,
            categoryIds: [news.Id],
            authorIds: [team.Id],
            tags: ["welcome", "v1"],
            isFeatured: true,
            cancellationToken);

        await EnsurePostAsync(
            db,
            title: "Editorial Workflow Basics",
            slug: "editorial-workflow-basics",
            summary: "A starter guide for organizing content in ContentHub.",
            content: "Use categories for navigation, authors for attribution, tags for discovery, and publishing states for workflow control.",
            createdById: admin.Id,
            categoryIds: [guides.Id],
            authorIds: [team.Id],
            tags: ["guides", "workflow"],
            isFeatured: false,
            cancellationToken);

        await EnsurePostAsync(
            db,
            title: "ContentHub v1 Release Notes",
            slug: "contenthub-v1-release-notes",
            summary: "The baseline feature set included in the first version.",
            content: "Version 1 establishes the API, database, authentication, authorization, CMS entities, local assets, notifications, search, audit logs, and automated tests.",
            createdById: admin.Id,
            categoryIds: [releases.Id],
            authorIds: [team.Id],
            tags: ["release-notes", "v1"],
            isFeatured: false,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Category> EnsureCategoryAsync(
        ContentHubDbContext db,
        string name,
        string slug,
        string description,
        int displayOrder,
        CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        if (category is not null)
        {
            return category;
        }

        category = new Category(
            name: name,
            slug: slug,
            description: description,
            parentCategoryId: null,
            displayOrder: displayOrder,
            isVisible: true);

        db.Categories.Add(category);

        return category;
    }

    private static async Task<Author> EnsureAuthorAsync(
        ContentHubDbContext db,
        string displayName,
        string slug,
        string bio,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var author = await db.Authors
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        if (author is not null)
        {
            return author;
        }

        author = new Author(
            displayName: displayName,
            slug: slug,
            bio: bio,
            avatarAssetId: null,
            isActive: true,
            userId: userId);

        db.Authors.Add(author);

        return author;
    }

    private static async Task EnsurePostAsync(
        ContentHubDbContext db,
        string title,
        string slug,
        string summary,
        string content,
        Guid createdById,
        IReadOnlyCollection<Guid> categoryIds,
        IReadOnlyCollection<Guid> authorIds,
        IReadOnlyCollection<string> tags,
        bool isFeatured,
        CancellationToken cancellationToken)
    {
        var exists = await db.Posts
            .AnyAsync(post => post.Slug == slug, cancellationToken);

        if (exists)
        {
            return;
        }

        var post = new Post(
            title: title,
            slug: slug,
            summary: summary,
            content: content,
            createdById: createdById);

        post.ReplaceCategories(categoryIds);
        post.ReplaceAuthors(authorIds);
        post.ReplaceTags(tags);
        post.Publish();

        if (isFeatured)
        {
            post.SetFeatured();
        }

        db.Posts.Add(post);
    }
}
