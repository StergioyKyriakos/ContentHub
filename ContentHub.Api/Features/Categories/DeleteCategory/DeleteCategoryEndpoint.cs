using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Categories.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
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
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(category => category.Id == id, ct);

        if (category is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(CategoryErrors.NotFound));
        }

        category.MarkAsDeleted();

        await db.SaveChangesAsync(ct);

        var response = new DeleteCategoryResponse();
        return Results.Ok(ApiResponse<DeleteCategoryResponse>.Ok(response));
    }
}