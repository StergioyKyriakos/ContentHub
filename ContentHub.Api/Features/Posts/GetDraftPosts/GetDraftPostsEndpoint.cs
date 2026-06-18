using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Entities.Posts;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.GetDraftPosts;

public sealed class GetDraftPostsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(PostEndpoints.GetDrafts, Handle)
            .WithTags("Posts")
            .WithName("GetDraftPosts")
            .RequireAuthorization(Policies.AuthorOrEditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [FromBody] GetDraftPostsQuery query,
        IValidator<GetDraftPostsQuery> validator,
        HttpContext httpContext,
        ICurrentUserProvider currentUserProvider,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        IQueryable<Post> dbQuery = db.Posts
            .AsNoTracking()
            .Where(post => post.Status == PostStatus.Draft);

        var isAdminOrEditor =
            httpContext.User.IsInRole(Roles.Admin) ||
            httpContext.User.IsInRole(Roles.Editor);

        if (!isAdminOrEditor)
        {
            if (currentUserProvider.UserId is null)
            {
                return ResultsFactory.Unauthorized();
            }

            dbQuery = dbQuery.Where(post => post.CreatedById == currentUserProvider.UserId.Value);
        }

        var totalCount = await dbQuery.CountAsync(ct);

        var posts = await dbQuery
            .OrderByDescending(post => post.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(post => new PostListItemDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Summary = post.Summary,
                Status = PostStatusDto.Draft,
                IsFeatured = post.IsFeatured,
                PublishedAtUtc = post.PublishedAtUtc,
                ScheduledForUtc = post.ScheduledForUtc,
                CoverAssetId = post.CoverAssetId,
                CreatedAtUtc = post.CreatedAtUtc,
                UpdatedAtUtc = post.UpdatedAtUtc
            })
            .ToListAsync(ct);

        var response = new GetDraftPostsResponse
        {
            Items = posts,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return Results.Ok(ApiResponse<GetDraftPostsResponse>.Ok(response));
    }
}
