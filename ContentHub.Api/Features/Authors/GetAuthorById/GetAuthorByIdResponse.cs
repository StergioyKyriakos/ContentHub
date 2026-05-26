using ContentHub.Data.Dtos.Authors;

namespace ContentHub.Api.Features.Authors.GetAuthorById;

public class GetAuthorByIdResponse
{
    public AuthorDto Author { get; set; } = null!;
}