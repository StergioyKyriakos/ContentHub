using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Categories.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(CategoryEndpoints.Delete, Handle)
            .WithTags("Categories")
            .WithName("DeleteCategory")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] DeleteCategoryCommand command,
        IValidator<DeleteCategoryCommand> validator,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (id != command.Id)
        {
            return ResultsFactory.BadRequest(
                "request.route_body_mismatch",
                "Route id and body id must match.");
        }

        var category = await db.Categories
            .FirstOrDefaultAsync(category => category.Id == command.Id, ct);

        if (category is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(CategoryErrors.NotFound));
        }

        var oldValues = new
        {
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentCategoryId,
            category.DisplayOrder,
            category.IsVisible
        };

        category.MarkAsDeleted();

        auditLogWriter.Add(
            action: AuditAction.CategoryDeleted,
            entityName: "Category",
            entityId: category.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                category.Id,
                category.IsDeleted,
                category.DeletedAtUtc
            });

        await db.SaveChangesAsync(ct);

        var response = new DeleteCategoryResponse();
        return Results.Ok(ApiResponse<DeleteCategoryResponse>.Ok(response));
    }
}
