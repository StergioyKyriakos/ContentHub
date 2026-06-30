using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Search.Shared;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Assets;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Assets;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Search.OpenSearch;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Search.SearchDocuments;

public sealed class SearchDocumentsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(SearchEndpoints.SearchDocuments, Handle)
            .WithTags("Search")
            .WithName("SearchDocuments")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] SearchDocumentsQuery query,
        IValidator<SearchDocumentsQuery> validator,
        ContentHubDbContext db,
        IFileUrlResolver fileUrlResolver,
        OpenSearchIndex openSearchIndex,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (openSearchIndex.IsOpenSearchEnabled())
        {
            return await SearchWithOpenSearchAsync(query, openSearchIndex, ct);
        }

        IQueryable<Asset> documentsQuery = db.Assets
            .AsNoTracking()
            .Where(asset => asset.Type == AssetType.Document);

        var hasSearch = !string.IsNullOrWhiteSpace(query.Q);
        var searchText = query.Q?.Trim();

        if (hasSearch)
        {
            documentsQuery = documentsQuery.Where(asset => asset.SearchVector.Matches(
                EF.Functions.WebSearchToTsQuery("simple", searchText!)));
        }

        if (query.Visibility.HasValue)
        {
            documentsQuery = documentsQuery.Where(asset => asset.Visibility == query.Visibility.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ContentType))
        {
            var pattern = $"%{query.ContentType.Trim()}%";

            documentsQuery = documentsQuery.Where(asset =>
                EF.Functions.ILike(asset.ContentType, pattern));
        }

        documentsQuery = ApplySorting(
            documentsQuery,
            query.SortBy,
            query.SortDirection,
            hasSearch,
            searchText);

        var totalItems = await documentsQuery.CountAsync(ct);

        var documents = await documentsQuery
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
                asset.Provider,
                asset.Type,
                asset.Visibility,
                asset.CreatedAtUtc
            })
            .ToListAsync(ct);

        var items = documents
            .Select(asset => new AssetSummaryDto
            {
                Id = asset.Id,
                FileName = asset.FileName,
                OriginalFileName = asset.OriginalFileName,
                ContentType = asset.ContentType,
                Size = asset.Size,
                Url = fileUrlResolver.ResolveUrl(asset.StoragePath, asset.Provider),
                Type = asset.Type,
                Visibility = asset.Visibility,
                CreatedAtUtc = asset.CreatedAtUtc
            })
            .ToList();

        var response = new SearchDocumentsResponse
        {
            Documents = PagedResponse<AssetSummaryDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems)
        };

        return Results.Ok(ApiResponse<SearchDocumentsResponse>.Ok(response));
    }

    private static async Task<IResult> SearchWithOpenSearchAsync(
        SearchDocumentsQuery query,
        OpenSearchIndex openSearchIndex,
        CancellationToken ct)
    {
        try
        {
            var result = await openSearchIndex.SearchDocumentsAsync(
                new OpenSearchDocumentSearchRequest
                {
                    Query = query.Q,
                    Visibility = query.Visibility,
                    ContentType = query.ContentType,
                    SortBy = query.SortBy,
                    SortDirection = query.SortDirection,
                    Page = query.Page,
                    PageSize = query.PageSize
                },
                ct);

            var items = result.Items
                .Select(hit => new AssetSummaryDto
                {
                    Id = hit.Document.Id,
                    FileName = hit.Document.FileName,
                    OriginalFileName = hit.Document.OriginalFileName,
                    ContentType = hit.Document.ContentType,
                    Size = hit.Document.Size,
                    Url = hit.Document.Url,
                    Type = hit.Document.Type,
                    Visibility = hit.Document.Visibility,
                    CreatedAtUtc = hit.Document.CreatedAtUtc,
                    Highlights = hit.Highlights
                })
                .ToList();

            var response = new SearchDocumentsResponse
            {
                Documents = PagedResponse<AssetSummaryDto>.Create(
                    items,
                    query.Page,
                    query.PageSize,
                    result.TotalCount)
            };

            return Results.Ok(ApiResponse<SearchDocumentsResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            return Results.Json(
                ApiResponse<object>.Fail(ApiError.Create(
                    "search.opensearch_unavailable",
                    $"OpenSearch document search failed: {ex.Message}")),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static IQueryable<Asset> ApplySorting(
        IQueryable<Asset> query,
        string? sortBy,
        string? sortDirection,
        bool hasSearch,
        string? searchText)
    {
        var sortField = sortBy?.ToLowerInvariant();

        if (hasSearch && IsRelevanceSort(sortField))
        {
            return query
                .OrderByDescending(asset => asset.SearchVector.Rank(
                    EF.Functions.WebSearchToTsQuery("simple", searchText!)))
                .ThenByDescending(asset => asset.CreatedAtUtc);
        }

        var descending = !string.Equals(
            sortDirection,
            "asc",
            StringComparison.OrdinalIgnoreCase);

        return sortField switch
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

    private static bool IsRelevanceSort(string? sortField)
    {
        return string.IsNullOrWhiteSpace(sortField) ||
               sortField is "relevance" or "createdat";
    }
}
