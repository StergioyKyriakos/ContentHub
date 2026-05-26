using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Authors;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Authors.UpdateAuthor;

public sealed class UpdateAuthorEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut(AuthorEndpoints.Update, Handle)
            .WithTags("Authors")
            .WithName("UpdateAuthor")
            .RequireAuthorization(Policies.EditorOrAdmin)
            .AddEndpointFilter<ValidationFilter<UpdateAuthorCommand>>();
    }

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] UpdateAuthorCommand request,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var author = await db.Authors
            .FirstOrDefaultAsync(author => author.Id == id, ct);

        if (author is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuthorErrors.NotFound));
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? AuthorSlugHelper.GenerateSlug(request.DisplayName)
            : AuthorSlugHelper.GenerateSlug(request.Slug);

        var slugExists = await db.Authors
            .AnyAsync(author => author.Id != id && author.Slug == slug, ct);

        if (slugExists)
        {
            return Results.Conflict(ApiResponse<DomainError>.Fail(AuthorErrors.SlugAlreadyExists));
        }

        author.Update(
            displayName: request.DisplayName,
            slug: slug,
            bio: request.Bio,
            avatarAssetId: request.AvatarAssetId,
            isActive: request.IsActive);

        await db.SaveChangesAsync(ct);

        var response = new UpdateAuthorResponse
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

        return Results.Ok(ApiResponse<UpdateAuthorResponse>.Ok(response));
    }
}