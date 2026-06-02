using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Categories.Shared;
using ContentHub.Data.Dtos.Categories;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(CategoryEndpoints.GetById, Handle)
            .WithTags("Categories")
            .WithName("GetCategoryById")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromBody] GetCategoryByIdQuery query,
        IValidator<GetCategoryByIdQuery> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var category = await db.Categories
            .AsNoTracking()
            .Include(category => category.ParentCategory)
            .Where(category => category.IsVisible)
            .FirstOrDefaultAsync(category => category.Id == query.Id, ct);

        if (category is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(CategoryErrors.NotFound));
        }

        var response = new GetCategoryByIdResponse
        {
            Category = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                DisplayOrder = category.DisplayOrder,
                IsVisible = category.IsVisible,
                CreatedAtUtc = category.CreatedAtUtc,
                UpdatedAtUtc = category.UpdatedAtUtc
            }
        };

        return Results.Ok(ApiResponse<GetCategoryByIdResponse>.Ok(response));
    }
}
