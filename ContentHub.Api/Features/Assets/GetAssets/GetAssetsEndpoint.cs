using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Assets.Shared;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Assets;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Assets.GetAssets;

public sealed class GetAssetsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AssetEndpoints.GetAll, Handle)
            .WithTags("Assets")
            .WithName("GetAssets")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAssetsQuery query,
        ContentHubDbContext db,
        IFileUrlResolver fileUrlResolver,
        CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var assetsQuery = db.Assets
            .AsNoTracking()
            .AsQueryable();

        if (query.Type.HasValue)
        {
            assetsQuery = assetsQuery.Where(asset => asset.Type == query.Type.Value);
        }

        if (query.Visibility.HasValue)
        {
            assetsQuery = assetsQuery.Where(asset => asset.Visibility == query.Visibility.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();

            assetsQuery = assetsQuery.Where(asset =>
                asset.OriginalFileName.ToLower().Contains(search) ||
                asset.FileName.ToLower().Contains(search));
        }

        var totalItems = await assetsQuery.CountAsync(ct);

        var assets = await assetsQuery
            .OrderByDescending(asset => asset.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(asset => new
            {
                asset.Id,
                asset.FileName,
                asset.OriginalFileName,
                asset.ContentType,
                asset.Size,
                asset.StoragePath,
                asset.Type,
                asset.Visibility,
                asset.CreatedAtUtc
            })
            .ToListAsync(ct);

        var items = assets
            .Select(asset => new AssetSummaryDto
            {
                Id = asset.Id,
                FileName = asset.FileName,
                OriginalFileName = asset.OriginalFileName,
                ContentType = asset.ContentType,
                Size = asset.Size,
                Url = fileUrlResolver.ResolveUrl(asset.StoragePath),
                Type = asset.Type,
                Visibility = asset.Visibility,
                CreatedAtUtc = asset.CreatedAtUtc
            })
            .ToList();

        var response = PagedResponse<AssetSummaryDto>.Create(
            items,
            page,
            pageSize,
            totalItems);

        return Results.Ok(ApiResponse<PagedResponse<AssetSummaryDto>>.Ok(response));
    }
}
