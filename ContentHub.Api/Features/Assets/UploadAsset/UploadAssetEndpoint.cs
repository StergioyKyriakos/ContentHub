using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Assets.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Assets;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Assets;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Storage;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Features.Assets.UploadAsset;

public sealed class UploadAssetEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AssetEndpoints.Upload, Handle)
            .WithTags("Assets")
            .WithName("UploadAsset")
            .RequireAuthorization(Policies.AuthenticatedOnly)
            .DisableAntiforgery()
            .Accepts<UploadAssetCommand>("multipart/form-data");
    }

    private static async Task<IResult> Handle(
        [FromForm] UploadAssetCommand command,
        IValidator<UploadAssetCommand> validator,
        ICurrentUserProvider currentUserProvider,
        IFileStorage fileStorage,
        IFileUrlResolver fileUrlResolver,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter, 
        CancellationToken ct)
    {
        if (currentUserProvider.UserId is null)
        {
            return ResultsFactory.Unauthorized();
        }

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        await using var stream = command.File.OpenReadStream();

        var storedFile = await fileStorage.SaveAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            ct);

        var assetType = AssetTypeResolver.Resolve(command.File.ContentType);

        var asset = new Asset(
            fileName: storedFile.FileName,
            originalFileName: storedFile.OriginalFileName,
            contentType: storedFile.ContentType,
            size: storedFile.Size,
            hash: storedFile.Hash,
            storagePath: storedFile.StoragePath,
            provider: storedFile.Provider,
            visibility: command.Visibility,
            type: assetType,
            uploadedById: currentUserProvider.UserId.Value);

        auditLogWriter.Add(
            action: AuditAction.AssetUploaded,
            entityName: "Asset",
            entityId: asset.Id.ToString(),
            newValues: new
            {
                asset.Id,
                asset.FileName,
                asset.OriginalFileName,
                asset.ContentType,
                asset.Size,
                asset.StoragePath,
                asset.Provider,
                asset.Visibility,
                asset.Type
            });

        db.Assets.Add(asset);

        await db.SaveChangesAsync(ct);

        var response = new UploadAssetResponse
        {
            Asset = new AssetDto
            {
                Id = asset.Id,
                FileName = asset.FileName,
                OriginalFileName = asset.OriginalFileName,
                ContentType = asset.ContentType,
                Size = asset.Size,
                Hash = asset.Hash,
                StoragePath = asset.StoragePath,
                Url = fileUrlResolver.ResolveUrl(asset.StoragePath, asset.Provider),
                Provider = asset.Provider,
                Visibility = asset.Visibility,
                Type = asset.Type,
                UploadedById = asset.UploadedById,
                CreatedAtUtc = asset.CreatedAtUtc,
                UpdatedAtUtc = asset.UpdatedAtUtc
            }
        };

        return Results.Ok(ApiResponse<UploadAssetResponse>.Ok(response));
    }
}
