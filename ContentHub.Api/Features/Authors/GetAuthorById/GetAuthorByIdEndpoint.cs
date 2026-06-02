using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Data.Dtos.Authors;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Authors.GetAuthorById;

public sealed class GetAuthorByIdEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuthorEndpoints.GetById, Handle)
            .WithTags("Authors")
            .WithName("GetAuthorById")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromBody] GetAuthorByIdQuery query,
        IValidator<GetAuthorByIdQuery> validator,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var author = await db.Authors
            .AsNoTracking()
            .Where(author => author.IsActive)
            .FirstOrDefaultAsync(author => author.Id == query.Id, ct);

        if (author is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuthorErrors.NotFound));
        }

        var response = new GetAuthorByIdResponse
        {
            Author = new AuthorDto
            {
                Id = author.Id,
                DisplayName = author.DisplayName,
                Slug = author.Slug,
                Bio = author.Bio,
                AvatarAssetId = author.AvatarAssetId,
                IsActive = author.IsActive,
                CreatedAtUtc = author.CreatedAtUtc,
                UpdatedAtUtc = author.UpdatedAtUtc
            }
        };

        return Results.Ok(ApiResponse<GetAuthorByIdResponse>.Ok(response));
    }
}
