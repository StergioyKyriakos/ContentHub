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

namespace ContentHub.Api.Features.Posts.RemoveFeaturedPost;

public sealed class RemoveFeaturedPostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(PostEndpoints.RemoveFeatured, Handle)
            .WithTags("Posts")
            .WithName("RemoveFeaturedPost")
            .RequireAuthorization(Policies.EditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [FromBody] RemoveFeaturedPostCommand command,
        IValidator<RemoveFeaturedPostCommand> validator,
        ContentHubDbContext db,
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

        post.RemoveFeatured();

        await db.SaveChangesAsync(ct);

        var response = new RemoveFeaturedPostResponse
        {
            Id = post.Id,
            Status = Enum.TryParse<PostStatusDto>(post.Status.ToString(), out var parsedStatus)
                ? parsedStatus
                : PostStatusDto.Published,
            PublishedAtUtc = post.PublishedAtUtc
        };

        return Results.Ok(ApiResponse<RemoveFeaturedPostResponse>.Ok(response));
    }
}