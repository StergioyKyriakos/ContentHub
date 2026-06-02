using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.SetFeaturedPost;

public sealed class SetFeaturedPostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(PostEndpoints.SetFeatured, Handle)
            .WithTags("Posts")
            .WithName("SetFeaturedPost")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [FromBody] SetFeaturedPostCommand command, 
        IValidator<SetFeaturedPostCommand> validator,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        if (!post.IsPublished)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.OnlyPublishedCanBeFeatured));
        }

        var oldValues = new
        {
            post.IsFeatured,
            post.FeaturedAtUtc
        };

        post.SetFeatured();

        auditLogWriter.Add(
            action: AuditAction.PostFeatured,
            entityName: "Post",
            entityId: post.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                post.IsFeatured,
                post.FeaturedAtUtc
            });

        await db.SaveChangesAsync(ct);

        var response = new SetFeaturedPostResponse
        {
            Id = post.Id,
            Status = Enum.TryParse<PostStatusDto>(post.Status.ToString(), out var parsedStatus)
                ? parsedStatus
                : PostStatusDto.Published,
            PublishedAtUtc = post.PublishedAtUtc
        };

        return Results.Ok(ApiResponse<SetFeaturedPostResponse>.Ok(response));
    }
}
