using ContentHub.Data.Dtos.Authors;

namespace ContentHub.Api.Features.Authors.GetAuthors;

public sealed class GetAuthorsResponse
{
    public IReadOnlyCollection<AuthorSummaryDto> Authors { get; set; } = [];
}