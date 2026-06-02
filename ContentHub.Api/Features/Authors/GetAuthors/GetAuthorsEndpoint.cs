using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Data.Dtos.Authors;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Authors.GetAuthors;

public sealed class GetAuthorsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuthorEndpoints.GetAll, Handle)
            .WithTags("Authors")
            .WithName("GetAuthors")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromBody] GetAuthorsQuery query,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var authorsQuery = db.Authors
            .AsNoTracking()
            .AsQueryable();

        if (!query.IncludeInactive)
        {
            authorsQuery = authorsQuery
                .Where(author => author.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var normalizedSearch = query.Search.Trim().ToLowerInvariant();

            authorsQuery = authorsQuery
                .Where(author =>
                    author.DisplayName.ToLower().Contains(normalizedSearch) ||
                    author.Slug.ToLower().Contains(normalizedSearch));
        }

        var authors = await authorsQuery
            .OrderBy(author => author.DisplayName)
            .Select(author => new AuthorSummaryDto
            {
                Id = author.Id,
                DisplayName = author.DisplayName,
                Slug = author.Slug,
                AvatarAssetId = author.AvatarAssetId,
                IsActive = author.IsActive
            })
            .ToListAsync(ct);

        var response = new GetAuthorsResponse
        {
            Authors = authors
        };

        return Results.Ok(ApiResponse<GetAuthorsResponse>.Ok(response));
    }
}
