using System.Text.Json;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Posts.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Posts;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Entities.Outbox;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Outbox;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Posts.PublishPost;

public sealed class PublishPostEndpoint : IEndpointDefinition
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(PostEndpoints.Publish, Handle)
            .WithTags("Posts")
            .WithName("PublishPost")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] PublishPostCommand command,
        IValidator<PublishPostCommand> validator,
        ContentHubDbContext db,
        ICurrentUserProvider currentUserProvider,
        IHttpContextAccessor httpContextAccessor,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (id != command.Id)
        {
            return ResultsFactory.BadRequest(
                "request.route_body_mismatch",
                "Route id and body id must match.");
        }

        var post = await db.Posts
            .Include(post => post.Categories)
            .Include(post => post.Authors)
            .ThenInclude(postAuthor => postAuthor.Author)
            .FirstOrDefaultAsync(post => post.Id == command.Id, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(PostErrors.NotFound));
        }

        if (post.IsArchived)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.CannotPublishArchived));
        }

        if (string.IsNullOrWhiteSpace(post.Title) ||
            string.IsNullOrWhiteSpace(post.Slug) ||
            string.IsNullOrWhiteSpace(post.Content))
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(
                ApiError.Create(
                    code: "posts.missing_required_fields",
                    message: "A post needs title, slug and content before it can be published.")));
        }

        if (post.Categories.Count == 0)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.CategoryRequired));
        }

        if (post.Authors.Count == 0)
        {
            return Results.BadRequest(ApiResponse<DomainError>.Fail(PostErrors.AuthorRequired));
        }
        
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var oldValues = new PostPublishedAuditValues
        {
            Status = post.Status.ToString(),
            PublishedAtUtc = post.PublishedAtUtc,
            ScheduledForUtc = post.ScheduledForUtc
        };

        post.Publish();

        var authorUserIds = post.Authors
            .Select(postAuthor => postAuthor.Author.UserId)
            .Where(userId => userId.HasValue)
            .Select(userId => userId!.Value)
            .Distinct()
            .ToList();

        if (authorUserIds.Count == 0)
        {
            authorUserIds.Add(post.CreatedById);
        }

        var httpContext = httpContextAccessor.HttpContext;
        var payload = new PostPublishedOutboxPayload
        {
            PostId = post.Id,
            PostTitle = post.Title,
            CreatedById = post.CreatedById,
            AuthorUserIds = authorUserIds,
            ActorUserId = currentUserProvider.UserId,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            OldValues = oldValues,
            NewValues = new PostPublishedAuditValues
            {
                Status = post.Status.ToString(),
                PublishedAtUtc = post.PublishedAtUtc,
                ScheduledForUtc = post.ScheduledForUtc
            }
        };

        db.OutboxMessages.Add(new OutboxMessage(
            OutboxMessageTypes.PostPublished,
            JsonSerializer.Serialize(payload, JsonOptions)));
        
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        var response = new PublishPostResponse
        {
            Id = post.Id,
            Status = PostStatusDto.Published,
            PublishedAtUtc = post.PublishedAtUtc
        };

        return Results.Ok(ApiResponse<PublishPostResponse>.Ok(response));
    }
}
