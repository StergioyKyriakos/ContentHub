using ContentHub.Data.Dtos.Categories;

namespace ContentHub.Api.Features.Categories.GetCategories;

public sealed class GetCategoriesResponse
{
    public IReadOnlyCollection<CategorySummaryDto> Categories { get; set; } = [];
}