using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Authors.DeleteAuthor;

public sealed class DeleteAuthorEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(AuthorEndpoints.Delete, Handle)
            .WithTags("Authors")
            .WithName("DeleteAuthor")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        Guid id,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var author = await db.Authors
            .FirstOrDefaultAsync(author => author.Id == id, ct);

        if (author is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuthorErrors.NotFound));
        }

        author.MarkAsDeleted();

        await db.SaveChangesAsync(ct);

        var response = new DeleteAuthorResponse();
        return Results.Ok(ApiResponse<DeleteAuthorResponse>.Ok(response));
    }
}