using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Categories.Shared;
using ContentHub.Data.Dtos.Categories;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Categories.GetCategories;

public sealed class GetCategoriesEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(CategoryEndpoints.GetAll, Handle)
            .WithTags("Categories")
            .WithName("GetCategories")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromBody] GetCategoriesQuery query,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var categoriesQuery = db.Categories
            .AsNoTracking()
            .AsQueryable();

        if (!query.IncludeHidden)
        {
            categoriesQuery = categoriesQuery
                .Where(category => category.IsVisible);
        }

        if (query.ParentCategoryId.HasValue)
        {
            categoriesQuery = categoriesQuery
                .Where(category => category.ParentCategoryId == query.ParentCategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var normalizedSearch = query.Search.Trim().ToLowerInvariant();

            categoriesQuery = categoriesQuery
                .Where(category =>
                    category.Name.ToLower().Contains(normalizedSearch) ||
                    category.Slug.ToLower().Contains(normalizedSearch));
        }

        var categories = await categoriesQuery
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new CategorySummaryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ParentCategoryId = category.ParentCategoryId,
                DisplayOrder = category.DisplayOrder,
                IsVisible = category.IsVisible
            })
            .ToListAsync(ct);

        var response = new GetCategoriesResponse
        {
            Categories = categories
        };

        return Results.Ok(ApiResponse<GetCategoriesResponse>.Ok(response));
    }
}
