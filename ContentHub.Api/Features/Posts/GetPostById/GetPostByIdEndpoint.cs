using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.GetPostById;

public sealed class GetPostByIdEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(PostEndpoints.GetById, Handle)
            .WithTags("Posts")
            .WithName("GetPostById")
            .RequireAuthorization(Policies.AuthorOrEditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] GetPostByIdQuery request,
        IValidator<GetPostByIdQuery> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (id != request.Id)
        {
            return ResultsFactory.BadRequest(
                "request.route_body_mismatch",
                "Route id and body id must match.");
        }

        var post = await db.Posts
            .AsNoTracking()
            .Include(post => post.Tags)
            .FirstOrDefaultAsync(post => post.Id == request.Id, ct);

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
            Status = Enum.Parse<PostStatusDto>(post.Status.ToString()),
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
