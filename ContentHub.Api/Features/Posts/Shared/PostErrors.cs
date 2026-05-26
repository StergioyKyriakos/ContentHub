using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Posts.Shared;

public static class PostErrors
{
    public static ApiError NotFound =>
        ApiError.Create(
            code: "posts.not_found",
            message: "Post was not found.");

    public static ApiError SlugAlreadyExists =>
        ApiError.Create(
            code: "posts.slug_already_exists",
            message: "A post with this slug already exists.");

    public static ApiError CategoryRequired =>
        ApiError.Create(
            code: "posts.category_required",
            message: "A post needs at least one category before it can be published.");

    public static ApiError AuthorRequired =>
        ApiError.Create(
            code: "posts.author_required",
            message: "A post needs at least one author before it can be published.");

    public static ApiError CategoryNotFound =>
        ApiError.Create(
            code: "posts.category_not_found",
            message: "One or more categories were not found.");

    public static ApiError AuthorNotFound =>
        ApiError.Create(
            code: "posts.author_not_found",
            message: "One or more authors were not found.");

    public static ApiError CannotPublishArchived =>
        ApiError.Create(
            code: "posts.cannot_publish_archived",
            message: "Archived posts cannot be published directly.");

    public static ApiError OnlyPublishedCanBeFeatured =>
        ApiError.Create(
            code: "posts.only_published_can_be_featured",
            message: "Only published posts can be featured.");

    public static ApiError ScheduledDateMustBeFuture =>
        ApiError.Create(
            code: "posts.scheduled_date_must_be_future",
            message: "Scheduled date must be in the future.");

    public static ApiError CannotEditPost =>
        ApiError.Create(
            code: "posts.cannot_edit_post",
            message: "You cannot edit this post.");
}