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

namespace ContentHub.Api.Features.Posts.UnpublishPost;

public sealed class UnpublishPostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(PostEndpoints.Unpublish, Handle)
            .WithTags("Posts")
            .WithName("UnpublishPost")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [FromBody] UnpublishPostCommand command,
        IValidator<UnpublishPostCommand> validator,
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

        var oldValues = new
        {
            post.Status,
            post.PublishedAtUtc,
            post.IsFeatured,
            post.FeaturedAtUtc
        };

        post.Unpublish();

        auditLogWriter.Add(
            action: AuditAction.PostUnpublished,
            entityName: "Post",
            entityId: post.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                post.Status,
                post.PublishedAtUtc,
                post.IsFeatured,
                post.FeaturedAtUtc
            });

        await db.SaveChangesAsync(ct);

        var response = new UnpublishPostResponse
        {
            Id = post.Id,
            Status = Enum.TryParse<PostStatusDto>(post.Status.ToString(), out var parsedStatus)
                ? parsedStatus
                : PostStatusDto.Draft,
            PublishedAtUtc = post.PublishedAtUtc
        };

        return Results.Ok(ApiResponse<UnpublishPostResponse>.Ok(response));
    }
}
