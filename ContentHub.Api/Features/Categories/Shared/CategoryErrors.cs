using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Categories.Shared;

public static class CategoryErrors
{
    public static ApiError NotFound =>
        ApiError.Create(
            code: "categories.not_found",
            message: "Category was not found.");

    public static ApiError SlugAlreadyExists =>
        ApiError.Create(
            code: "categories.slug_already_exists",
            message: "A category with this slug already exists.");

    public static ApiError ParentCategoryNotFound =>
        ApiError.Create(
            code: "categories.parent_not_found",
            message: "Parent category was not found.");

    public static ApiError InvalidParentCategory =>
        ApiError.Create(
            code: "categories.invalid_parent",
            message: "A category cannot be its own parent category.");
}