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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
            .Accepts<IFormFile>("multipart/form-data");
    }

    private static async Task<IResult> Handle(
        IFormFile file,
        [FromForm] AssetVisibility visibility,
        ICurrentUserProvider currentUserProvider,
        IFileStorage fileStorage,
        IFileUrlResolver fileUrlResolver,
        IOptions<StorageOptions> storageOptions,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter, 
        CancellationToken ct)
    {
        if (currentUserProvider.UserId is null)
        {
            return Results.Unauthorized();
        }

        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(AssetErrors.FileRequired));
        }

        if (file.Length > storageOptions.Value.MaxFileSizeBytes)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(AssetErrors.FileTooLarge));
        }

        var allowedContentTypes = storageOptions.Value.AllowedContentTypes;

        if (!allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(AssetErrors.ContentTypeNotAllowed));
        }

        await using var stream = file.OpenReadStream();

        var storedFile = await fileStorage.SaveAsync(
            stream,
            file.FileName,
            file.ContentType,
            ct);

        var assetType = AssetTypeResolver.Resolve(file.ContentType);

        var asset = new Asset(
            fileName: storedFile.FileName,
            originalFileName: storedFile.OriginalFileName,
            contentType: storedFile.ContentType,
            size: storedFile.Size,
            hash: storedFile.Hash,
            storagePath: storedFile.StoragePath,
            provider: StorageProvider.Local,
            visibility: visibility,
            type: assetType,
            uploadedById: currentUserProvider.UserId.Value);

        db.Assets.Add(asset);

        await db.SaveChangesAsync(ct);
        
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
                Url = fileUrlResolver.ResolveUrl(asset.StoragePath),
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