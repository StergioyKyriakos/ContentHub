using ContentHub.Data.Dtos.Categories;

namespace ContentHub.Api.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryResponse
{
    public CategoryDto Category { get; set; } = null!;
}