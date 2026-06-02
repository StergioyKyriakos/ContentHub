using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Search.Shared;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Entities.Assets;
using ContentHub.Data.Dtos.Assets;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Search.SearchAssets;

public sealed class SearchAssetsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(SearchEndpoints.SearchAssets, Handle)
            .WithTags("Search")
            .WithName("SearchAssets")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] SearchAssetsQuery query,
        IValidator<SearchAssetsQuery> validator,
        ContentHubDbContext db,
        IFileUrlResolver fileUrlResolver,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        IQueryable<Asset> assetsQuery = db.Assets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var pattern = $"%{query.Q.Trim()}%";

            assetsQuery = assetsQuery.Where(asset =>
                EF.Functions.ILike(asset.FileName, pattern) ||
                EF.Functions.ILike(asset.OriginalFileName, pattern) ||
                EF.Functions.ILike(asset.ContentType, pattern) ||
                EF.Functions.ILike(asset.StoragePath, pattern));
        }

        if (query.Type.HasValue)
        {
            assetsQuery = assetsQuery.Where(asset => asset.Type == query.Type.Value);
        }

        if (query.Visibility.HasValue)
        {
            assetsQuery = assetsQuery.Where(asset => asset.Visibility == query.Visibility.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ContentType))
        {
            var pattern = $"%{query.ContentType.Trim()}%";

            assetsQuery = assetsQuery.Where(asset =>
                EF.Functions.ILike(asset.ContentType, pattern));
        }

        assetsQuery = ApplySorting(assetsQuery, query.SortBy, query.SortDirection);

        var totalItems = await assetsQuery.CountAsync(ct);

        var assets = await assetsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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

        var response = new SearchAssetsResponse
        {
            Assets = PagedResponse<AssetSummaryDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems)
        };

        return Results.Ok(ApiResponse<SearchAssetsResponse>.Ok(response));
    }

    private static IQueryable<Asset> ApplySorting(
        IQueryable<Asset> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = !string.Equals(
            sortDirection,
            "asc",
            StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "filename" => descending
                ? query.OrderByDescending(asset => asset.FileName)
                : query.OrderBy(asset => asset.FileName),

            "originalfilename" => descending
                ? query.OrderByDescending(asset => asset.OriginalFileName)
                : query.OrderBy(asset => asset.OriginalFileName),

            "size" => descending
                ? query.OrderByDescending(asset => asset.Size)
                : query.OrderBy(asset => asset.Size),

            "type" => descending
                ? query.OrderByDescending(asset => asset.Type)
                : query.OrderBy(asset => asset.Type),

            "createdat" => descending
                ? query.OrderByDescending(asset => asset.CreatedAtUtc)
                : query.OrderBy(asset => asset.CreatedAtUtc),

            _ => descending
                ? query.OrderByDescending(asset => asset.CreatedAtUtc)
                : query.OrderBy(asset => asset.CreatedAtUtc)
        };
    }
}
