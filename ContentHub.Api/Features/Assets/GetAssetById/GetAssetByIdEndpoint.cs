using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Assets.Shared;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Data.Entities.Assets; 
using ContentHub.Data.Dtos.Assets;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Assets.GetAssetById;

public sealed class GetAssetByIdEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AssetEndpoints.GetById, Handle)
            .WithTags("Assets")
            .WithName("GetAssetById")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAssetByIdQuery query,
        IValidator<GetAssetByIdQuery> validator,
        ContentHubDbContext db,
        IFileUrlResolver fileUrlResolver,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        IQueryable<Asset> dbQuery = db.Assets;
        
        var asset = await dbQuery
            .AsNoTracking()
            .FirstOrDefaultAsync(asset => asset.Id == query.Id, ct);

        if (asset is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AssetErrors.NotFound));
        }

        var assetDto = new AssetDto
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
        };

        var response = new GetAssetByIdResponse
        {
            Asset = assetDto
        };

        return Results.Ok(ApiResponse<GetAssetByIdResponse>.Ok(response));
    }
}
