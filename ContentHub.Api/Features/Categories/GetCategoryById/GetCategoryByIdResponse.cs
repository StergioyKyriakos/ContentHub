using ContentHub.Data.Dtos.Categories;

namespace ContentHub.Api.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdResponse
{
    public CategoryDto Category { get; set; } = null!;
}