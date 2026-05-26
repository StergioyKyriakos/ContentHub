namespace ContentHub.Data.Dtos.Common;

public sealed class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public static PagedResponse<T> Create(
        IReadOnlyCollection<T> items,
        int page,
        int pageSize,
        int totalItems)
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResponse<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}