using ContentHub.Data.Dtos.Categories;

namespace ContentHub.Api.Features.Categories.CreateCategory;

public sealed class CreateCategoryResponse
{
    public CategoryDto Category { get; set; } = null!;
}