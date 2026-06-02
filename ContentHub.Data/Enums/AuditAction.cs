namespace ContentHub.Data.Enums;

public enum AuditAction
{
    UserRegistered = 1,
    UserLoggedIn = 2,

    CategoryCreated = 100,
    CategoryUpdated = 101,
    CategoryDeleted = 102,

    AuthorCreated = 200,
    AuthorUpdated = 201,
    AuthorDeleted = 202,

    PostCreated = 300,
    PostUpdated = 301,
    PostDeleted = 302,
    PostPublished = 303,
    PostUnpublished = 304,
    PostArchived = 305,
    PostScheduled = 306,
    PostFeatured = 307,
    PostUnfeatured = 308,

    AssetUploaded = 400,
    AssetDeleted = 401,
    AssetAttachedToPost = 402,
    AssetDetachedFromPost = 403
}
