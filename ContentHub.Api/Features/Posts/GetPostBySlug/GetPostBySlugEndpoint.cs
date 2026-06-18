using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.GetPostBySlug;

public sealed class GetPostBySlugEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(PostEndpoints.PublicPostBySlug, Handle)
            .WithTags("Public Posts")
            .WithName("GetPostBySlug")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        string slug,
        [FromBody] GetPostBySlugQuery query,
        IValidator<GetPostBySlugQuery> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (!string.Equals(slug, query.Slug, StringComparison.OrdinalIgnoreCase))
        {
            return ResultsFactory.BadRequest(
                "request.route_body_mismatch",
                "Route slug and body slug must match.");
        }

        var post = await db.Posts
            .AsNoTracking()
            .Include(post => post.Tags)
            .Where(post => post.Status == PostStatus.Published)
            .FirstOrDefaultAsync(post => post.Slug == query.Slug, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        var response = new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            Content = post.Content,
            Status = PostStatusDto.Published,
            IsFeatured = post.IsFeatured,
            FeaturedAtUtc = post.FeaturedAtUtc,
            PublishedAtUtc = post.PublishedAtUtc,
            ScheduledForUtc = post.ScheduledForUtc,
            CoverAssetId = post.CoverAssetId,
            CreatedById = post.CreatedById,
            UpdatedById = post.UpdatedById,
            Tags = post.Tags.Select(tag => tag.Name).ToArray(),
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc
        };

        return Results.Ok(ApiResponse<PostDto>.Ok(response));
    }
}
