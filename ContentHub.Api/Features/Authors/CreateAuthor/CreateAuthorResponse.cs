using ContentHub.Data.Dtos.Authors;

namespace ContentHub.Api.Features.Authors.CreateAuthor;

public class CreateAuthorResponse
{
    public AuthorDto Author { get; set; } = null!;
}