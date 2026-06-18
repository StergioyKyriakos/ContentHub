using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Assets.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Assets.DetachAssetFromPost;

public sealed class DetachAssetFromPostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(AssetEndpoints.DetachFromPost, Handle)
            .WithTags("Assets")
            .WithName("DetachAssetFromPost")
            .RequireAuthorization(Policies.AuthorOrEditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        Guid postId,
        Guid assetId,
        [FromBody] DetachAssetFromPostCommand command,
        IValidator<DetachAssetFromPostCommand> validator,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (postId != command.PostId || assetId != command.AssetId)
        {
            return ResultsFactory.BadRequest(
                "request.route_body_mismatch",
                "Route ids and body ids must match.");
        }

        var post = await db.Posts
            .Include(post => post.Assets)
            .FirstOrDefaultAsync(post => post.Id == command.PostId, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AssetErrors.PostNotFound));
        }

        var oldValues = post.Assets
            .Where(asset => asset.AssetId == command.AssetId)
            .Select(asset => new
            {
                asset.PostId,
                asset.AssetId,
                asset.DisplayOrder
            })
            .FirstOrDefault();

        post.DetachAsset(command.AssetId);

        auditLogWriter.Add(
            action: AuditAction.AssetDetachedFromPost,
            entityName: "PostAsset",
            entityId: $"{command.PostId}:{command.AssetId}",
            oldValues: oldValues);

        await db.SaveChangesAsync(ct);

        var response = new DetachAssetFromPostResponse();
        return Results.Ok(ApiResponse<DetachAssetFromPostResponse>.Ok(response));
    }
}
