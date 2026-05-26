namespace ContentHub.Data.Dtos.Common;

public sealed class PaginationRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public SortDirectionDto SortDirection { get; set; } = SortDirectionDto.Desc;

    public int Skip => (Page - 1) * PageSize;
}