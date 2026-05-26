using ContentHub.Data.Dtos.Authors;

namespace ContentHub.Api.Features.Authors.UpdateAuthor;

public class UpdateAuthorResponse
{
    public AuthorDto Author { get; set; } = null!;
}