using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Data.Entities.Posts;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
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
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
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

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var searchKeyword = query.Q.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(post => post.Title.ToLower().Contains(searchKeyword) || 
                                            post.Summary!.ToLower().Contains(searchKeyword));
        }

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(post => post.Categories.Any(c => c.CategoryId == query.CategoryId.Value));
        }

        if (query.AuthorId.HasValue)
        {
            dbQuery = dbQuery.Where(post => post.Authors.Any(a => a.AuthorId == query.AuthorId.Value) || 
                                            post.CreatedById == query.AuthorId.Value);
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
}
