using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Assets.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Assets.DetachAssetFromPost;

public sealed class DetachAssetFromPostEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(AssetEndpoints.DetachFromPost, Handle)
            .WithTags("Assets")
            .WithName("DetachAssetFromPost")
            .RequireAuthorization(Policies.AuthorOrEditorOrAdmin);
    }

    private static async Task<IResult> Handle(
        [AsParameters] DetachAssetFromPostCommand command,
        IValidator<DetachAssetFromPostCommand> validator,
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

        post.DetachAsset(command.AssetId);

        await db.SaveChangesAsync(ct);

        var response = new DetachAssetFromPostResponse();
        return Results.Ok(ApiResponse<DetachAssetFromPostResponse>.Ok(response));
    }
}