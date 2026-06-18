using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Authors.GetAuthorPosts;

public sealed class GetAuthorPostsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuthorEndpoints.GetPosts, Handle)
            .WithTags("Authors")
            .WithName("GetAuthorPosts")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] GetAuthorPostsQuery query,
        IValidator<GetAuthorPostsQuery> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (id != query.Id)
        {
            return ResultsFactory.BadRequest(
                "request.route_body_mismatch",
                "Route id and body id must match.");
        }

        var authorExists = await db.Authors
            .AsNoTracking()
            .AnyAsync(author => author.Id == query.Id && author.IsActive, ct);

        if (!authorExists)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuthorErrors.NotFound));
        }

        var posts = await db.Posts
            .AsNoTracking()
            .Where(post => post.Status == PostStatus.Published)
            .Where(post => post.Authors.Any(author => author.AuthorId == query.Id))
            .OrderByDescending(post => post.PublishedAtUtc)
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

        var response = new GetAuthorPostsResponse
        {
            AuthorId = query.Id,
            Posts = posts
        };

        return Results.Ok(ApiResponse<GetAuthorPostsResponse>.Ok(response));
    }
}
