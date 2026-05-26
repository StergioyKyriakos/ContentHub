using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
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
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var authorExists = await db.Authors
            .AsNoTracking()
            .AnyAsync(author => author.Id == id && author.IsActive, ct);

        if (!authorExists)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuthorErrors.NotFound));
        }

        var response = new GetAuthorPostsResponse
        {
            AuthorId = id,
            Posts = []
        };

        return Results.Ok(ApiResponse<GetAuthorPostsResponse>.Ok(response));
    }
}