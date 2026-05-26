using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Features.Search.Shared;

public static class SearchErrors
{
    public static ApiError InvalidSortBy =>
        ApiError.Create(
            code: "search.invalid_sort_by",
            message: "Invalid sort field.");

    public static ApiError InvalidSortDirection =>
        ApiError.Create(
            code: "search.invalid_sort_direction",
            message: "Invalid sort direction.");
}