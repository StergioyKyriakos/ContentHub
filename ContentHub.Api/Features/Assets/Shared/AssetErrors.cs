using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Assets.Shared;

public static class AssetErrors
{
    public static ApiError NotFound =>
        ApiError.Create(
            code: "assets.not_found",
            message: "Asset was not found.");

    public static ApiError FileRequired =>
        ApiError.Create(
            code: "assets.file_required",
            message: "A file is required.");

    public static ApiError FileTooLarge =>
        ApiError.Create(
            code: "assets.file_too_large",
            message: "The uploaded file exceeds the configured maximum size.");

    public static ApiError ContentTypeNotAllowed =>
        ApiError.Create(
            code: "assets.content_type_not_allowed",
            message: "This content type is not allowed.");

    public static ApiError UsedByPublishedPost =>
        ApiError.Create(
            code: "assets.used_by_published_post",
            message: "This asset is used by a published post and cannot be deleted unless force=true.");

    public static ApiError PostNotFound =>
        ApiError.Create(
            code: "assets.post_not_found",
            message: "Post was not found.");

    public static ApiError AlreadyAttached =>
        ApiError.Create(
            code: "assets.already_attached",
            message: "Asset is already attached to this post.");
}