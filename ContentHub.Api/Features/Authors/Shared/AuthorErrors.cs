using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Authors.Shared;

public static class AuthorErrors
{
    public static ApiError NotFound =>
        ApiError.Create(
            code: "authors.not_found",
            message: "Author was not found.");

    public static ApiError SlugAlreadyExists =>
        ApiError.Create(
            code: "authors.slug_already_exists",
            message: "An author with this slug already exists.");
}