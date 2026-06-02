using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Data.Entities.Posts;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.GetPublishedPosts;

public sealed class GetPublishedPostsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(PostEndpoints.PublicPosts, Handle)
            .WithTags("Public Posts")
            .WithName("GetPublishedPosts")
            .AllowAnonymous();
    }

   private static async Task<IResult> Handle(
        [FromBody] GetPublishedPostsQuery query,
        IValidator<GetPublishedPostsQuery> validator,
        ContentHubDbContext db, 
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        IQueryable<Post> dbQuery = db.Posts
            .AsNoTracking()
            .Where(post => post.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            dbQuery = dbQuery.Where(post => post.Title.Contains(query.Search) || 
                                            post.Summary!.Contains(query.Search));
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

        var totalCount = await dbQuery.CountAsync(ct);

        var posts = await dbQuery
            .OrderByDescending(post => post.PublishedAtUtc)
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

        var response = new GetPublishedPostsResponse
        {
            Items = posts,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return Results.Ok(ApiResponse<GetPublishedPostsResponse>.Ok(response));
    }
}
