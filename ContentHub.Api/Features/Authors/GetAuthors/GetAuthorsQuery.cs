namespace ContentHub.Api.Features.Authors.GetAuthors;

public sealed class GetAuthorsQuery
{
    public bool IncludeInactive { get; set; } = false;

    public string? Search { get; set; }
}
