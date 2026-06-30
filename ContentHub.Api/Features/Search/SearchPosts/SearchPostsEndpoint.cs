using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Data.Entities.Posts;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Search.OpenSearch;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace ContentHub.Api.Features.Search.SearchPosts;

public sealed class SearchPostsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/search/posts", Handle)
            .WithTags("Search")
            .WithName("SearchPosts")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromBody] SearchPostsQuery query,
        IValidator<SearchPostsQuery> validator,
        ContentHubDbContext db,
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

        IQueryable<Post> dbQuery = db.Posts.AsNoTracking();

        if (!query.IncludeUnpublished)
        {
            dbQuery = dbQuery.Where(post => post.Status == PostStatus.Published);
        }
        else if (query.Status.HasValue)
        {
            if (Enum.TryParse<PostStatus>(query.Status.Value.ToString(), out var databaseStatus))
            {
                dbQuery = dbQuery.Where(post => post.Status == databaseStatus);
            }
        }

        var hasSearch = !string.IsNullOrWhiteSpace(query.Q);
        var searchText = query.Q?.Trim();

        if (hasSearch)
        {
            dbQuery = dbQuery.Where(post => post.SearchVector.Matches(
                EF.Functions.WebSearchToTsQuery("english", searchText!)));
        }

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(post => post.Categories.Any(c => c.CategoryId == query.CategoryId.Value));
        }

        if (query.AuthorId.HasValue)
        {
            dbQuery = dbQuery.Where(post => post.Authors.Any(a => a.AuthorId == query.AuthorId.Value));
        }

        if (query.IsFeatured.HasValue)
        {
            dbQuery = dbQuery.Where(post => post.IsFeatured == query.IsFeatured.Value);
        }

        if (query.PublishedFrom.HasValue)
        {
            dbQuery = dbQuery.Where(post => post.PublishedAtUtc >= query.PublishedFrom.Value);
        }

        if (query.PublishedTo.HasValue)
        {
            dbQuery = dbQuery.Where(post => post.PublishedAtUtc <= query.PublishedTo.Value);
        }

        var totalCount = await dbQuery.CountAsync(ct);

        var sortField = query.SortBy?.Trim().ToLowerInvariant();
        var isDescending = query.SortDirection?.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase) ?? true;

        if (hasSearch && IsRelevanceSort(sortField))
        {
            dbQuery = dbQuery
                .OrderByDescending(post => post.SearchVector.Rank(
                    EF.Functions.WebSearchToTsQuery("english", searchText!)))
                .ThenByDescending(post => post.PublishedAtUtc)
                .ThenByDescending(post => post.CreatedAtUtc);
        }
        else
        {
            Expression<Func<Post, object>> sortExpression = sortField switch
            {
                "title" => post => post.Title,
                "createdat" => post => post.CreatedAtUtc,
                "isfeatured" => post => post.IsFeatured,
                _ => post => post.PublishedAtUtc!
            };

            dbQuery = isDescending
                ? dbQuery.OrderByDescending(sortExpression)
                : dbQuery.OrderBy(sortExpression);
        }

        var posts = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(post => new PostSummaryDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                IsFeatured = post.IsFeatured,
                PublishedAtUtc = post.PublishedAtUtc,
                CoverAssetId = post.CoverAssetId
            })
            .ToListAsync(ct);

        var response = new SearchPostsResponse
        {
            Items = posts,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return Results.Ok(ApiResponse<SearchPostsResponse>.Ok(response));
    }

    private static bool IsRelevanceSort(string? sortField)
    {
        return string.IsNullOrWhiteSpace(sortField) ||
               sortField is "relevance" or "publishedat";
    }

    private static async Task<IResult> SearchWithOpenSearchAsync(
        SearchPostsQuery query,
        OpenSearchIndex openSearchIndex,
        CancellationToken ct)
    {
        try
        {
            var result = await openSearchIndex.SearchPostsAsync(
                new OpenSearchPostSearchRequest
                {
                    Query = query.Q,
                    CategoryId = query.CategoryId,
                    AuthorId = query.AuthorId,
                    Status = query.Status.HasValue
                        ? Enum.Parse<PostStatus>(query.Status.Value.ToString())
                        : null,
                    PublishedFrom = query.PublishedFrom,
                    PublishedTo = query.PublishedTo,
                    IsFeatured = query.IsFeatured,
                    SortBy = query.SortBy,
                    SortDirection = query.SortDirection,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    IncludeUnpublished = query.IncludeUnpublished
                },
                ct);

            var response = new SearchPostsResponse
            {
                Items = result.Items
                    .Select(hit => new PostSummaryDto
                    {
                        Id = hit.Document.Id,
                        Title = hit.Document.Title,
                        Slug = hit.Document.Slug,
                        Summary = hit.Document.Summary,
                        IsFeatured = hit.Document.IsFeatured,
                        PublishedAtUtc = hit.Document.PublishedAtUtc,
                        CoverAssetId = hit.Document.CoverAssetId,
                        Highlights = hit.Highlights
                    })
                    .ToList(),
                PageNumber = query.Page,
                PageSize = query.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = (int)Math.Ceiling(result.TotalCount / (double)query.PageSize)
            };

            return Results.Ok(ApiResponse<SearchPostsResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            return Results.Json(
                ApiResponse<object>.Fail(ApiError.Create(
                    "search.opensearch_unavailable",
                    $"OpenSearch search failed: {ex.Message}")),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
