using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
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
        [FromBody] GetAuthorPostsQuery query,
        IValidator<GetAuthorPostsQuery> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var authorExists = await db.Authors
            .AsNoTracking()
            .AnyAsync(author => author.Id == query.Id && author.IsActive, ct);

        if (!authorExists)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuthorErrors.NotFound));
        }

        var response = new GetAuthorPostsResponse
        {
            AuthorId = query.Id,
            Posts = []
        };

        return Results.Ok(ApiResponse<GetAuthorPostsResponse>.Ok(response));
    }
}
