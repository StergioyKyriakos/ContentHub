using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Assets.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Assets.AttachAssetToPost;

public sealed class AttachAssetToPostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AssetEndpoints.AttachToPost, Handle)
            .WithTags("Assets")
            .WithName("AttachAssetToPost")
            .RequireAuthorization(Policies.AuthorOrEditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [AsParameters] AttachAssetToPostCommand command,
        IValidator<AttachAssetToPostCommand> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var post = await db.Posts
            .Include(post => post.Assets)
            .FirstOrDefaultAsync(post => post.Id == command.PostId, ct);

        if (post is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AssetErrors.PostNotFound));
        }

        var assetExists = await db.Assets
            .AnyAsync(asset => asset.Id == command.AssetId, ct);

        if (!assetExists)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AssetErrors.NotFound));
        }

        if (post.Assets.Any(asset => asset.AssetId == command.AssetId))
        {
            return Results.Conflict(ApiResponse<DomainError>.Fail(AssetErrors.AlreadyAttached));
        }

        var displayOrder = post.Assets.Count;

        post.AttachAsset(command.AssetId, displayOrder);

        await db.SaveChangesAsync(ct);

        var response = new AttachAssetToPostResponse();
        return Results.Ok(ApiResponse<AttachAssetToPostResponse>.Ok(response));
    }
}