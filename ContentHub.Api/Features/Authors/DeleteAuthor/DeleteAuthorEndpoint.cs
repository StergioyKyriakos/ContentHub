using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
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
        [FromBody] DeleteAuthorCommand command,
        IValidator<DeleteAuthorCommand> validator,
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

        var author = await db.Authors
            .FirstOrDefaultAsync(author => author.Id == command.Id, ct);

        if (author is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuthorErrors.NotFound));
        }

        var oldValues = new
        {
            author.Id,
            author.DisplayName,
            author.Slug,
            author.Bio,
            author.AvatarAssetId,
            author.IsActive
        };

        author.MarkAsDeleted();

        auditLogWriter.Add(
            action: AuditAction.AuthorDeleted,
            entityName: "Author",
            entityId: author.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                author.Id,
                author.IsDeleted,
                author.DeletedAtUtc
            });

        await db.SaveChangesAsync(ct);

        var response = new DeleteAuthorResponse();
        return Results.Ok(ApiResponse<DeleteAuthorResponse>.Ok(response));
    }
}
