using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Categories.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Categories;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut(CategoryEndpoints.Update, Handle)
            .WithTags("Categories")
            .WithName("UpdateCategory")
            .RequireAuthorization(Policies.EditorOrAdmin)
            .AddEndpointFilter<ValidationFilter<UpdateCategoryCommand>>();
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] UpdateCategoryCommand request,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(category => category.Id == id, ct);

        if (category is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(CategoryErrors.NotFound));
        }

        if (request.ParentCategoryId == id)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(CategoryErrors.InvalidParentCategory));
        }

        string? parentCategoryName = null;

        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == request.ParentCategoryId.Value, ct);

            if (parentCategory is null)
            {
                return Results.BadRequest(ApiResponse<DomainError>.Fail(CategoryErrors.ParentCategoryNotFound));
            }

            parentCategoryName = parentCategory.Name;
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Name)
            : SlugHelper.GenerateSlug(request.Slug);

        var slugExists = await db.Categories
            .AnyAsync(category => category.Id != id && category.Slug == slug, ct);

        if (slugExists)
        {
            return Results.Conflict(ApiResponse<DomainError>.Fail(CategoryErrors.SlugAlreadyExists));
        }

        category.Update(
            name: request.Name,
            slug: slug,
            description: request.Description,
            parentCategoryId: request.ParentCategoryId,
            displayOrder: request.DisplayOrder,
            isVisible: request.IsVisible);

        await db.SaveChangesAsync(ct);

        var response = new UpdateCategoryResponse
        {
            Category = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = parentCategoryName,
                DisplayOrder = category.DisplayOrder,
                IsVisible = category.IsVisible,
                CreatedAtUtc = category.CreatedAtUtc,
                UpdatedAtUtc = category.UpdatedAtUtc
            }
        };

        return Results.Ok(ApiResponse<UpdateCategoryResponse>.Ok(response));
    }
}