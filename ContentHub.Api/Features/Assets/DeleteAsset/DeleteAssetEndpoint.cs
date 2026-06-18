using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Assets.Shared;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Assets.DeleteAsset;

public sealed class DeleteAssetEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(AssetEndpoints.Delete, Handle)
            .WithTags("Assets")
            .WithName("DeleteAsset")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] DeleteAssetCommand command,
        IValidator<DeleteAssetCommand> validator,
        ContentHubDbContext db,
        IFileStorageFactory fileStorageFactory,
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

        var asset = await db.Assets
            .FirstOrDefaultAsync(asset => asset.Id == command.Id, ct);

        if (asset is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AssetErrors.NotFound));
        }

        var usedByPublishedPost = await db.Posts
            .AnyAsync(post =>
                post.Status == PostStatus.Published &&
                (
                    post.CoverAssetId == command.Id ||
                    post.Assets.Any(postAsset => postAsset.AssetId == command.Id)
                ),
                ct);

        if (usedByPublishedPost && !command.Force)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(AssetErrors.UsedByPublishedPost));
        }

        var oldValues = new
        {
            asset.Id,
            asset.FileName,
            asset.OriginalFileName,
            asset.StoragePath,
            asset.Visibility,
            asset.Type
        };
        
        asset.MarkAsDeleted();

        auditLogWriter.Add(
            action: AuditAction.AssetDeleted,
            entityName: "Asset",
            entityId: asset.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                asset.Id,
                asset.IsDeleted,
                asset.DeletedAtUtc
            });
        
        var fileStorage = fileStorageFactory.GetForProvider(asset.Provider);

        await fileStorage.DeleteAsync(asset.StoragePath, ct);

        await db.SaveChangesAsync(ct);

        var response = new DeleteAssetResponse();

        return Results.Ok(ApiResponse<DeleteAssetResponse>.Ok(response));
    }
}
