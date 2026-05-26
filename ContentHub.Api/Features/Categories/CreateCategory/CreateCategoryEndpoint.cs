using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Categories.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Categories;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Categories;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Categories.CreateCategory;

public sealed class CreateCategoryEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(CategoryEndpoints.Create, Handle)
            .WithTags("Categories")
            .WithName("CreateCategory")
            .RequireAuthorization(Policies.EditorOrAdmin)
            .AddEndpointFilter<ValidationFilter<CreateCategoryCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateCategoryCommand request,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Name)
            : SlugHelper.GenerateSlug(request.Slug);

        var slugExists = await db.Categories
            .AnyAsync(category => category.Slug == slug, ct);

        if (slugExists)
        {
            return Results.Conflict(ApiResponse<DomainError>.Fail(CategoryErrors.SlugAlreadyExists));
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

        var category = new Category(
            name: request.Name,
            slug: slug,
            description: request.Description,
            parentCategoryId: request.ParentCategoryId,
            displayOrder: request.DisplayOrder,
            isVisible: request.IsVisible);

        db.Categories.Add(category);

        auditLogWriter.Add(
            action: AuditAction.CategoryCreated,
            entityName: "Category",
            entityId: category.Id.ToString(),
            newValues: new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.ParentCategoryId,
                category.IsVisible
            });
        
        await db.SaveChangesAsync(ct);

        var response = new CreateCategoryResponse
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

        return Results.Created(
            $"/api/categories/{category.Id}",
            ApiResponse<CreateCategoryResponse>.Ok(response));
    }
}