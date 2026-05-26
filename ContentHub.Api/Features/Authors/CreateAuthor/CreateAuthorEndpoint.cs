using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Authors.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Authors;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Authors;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Authors.CreateAuthor;

public sealed class CreateAuthorEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthorEndpoints.Create, Handle)
            .WithTags("Authors")
            .WithName("CreateAuthor")
            .RequireAuthorization(Policies.EditorOrAdmin)
            .AddEndpointFilter<ValidationFilter<CreateAuthorCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateAuthorCommand request,
        ContentHubDbContext db,
        AuditLogWriter auditLogWriter,
        CancellationToken ct)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? AuthorSlugHelper.GenerateSlug(request.DisplayName)
            : AuthorSlugHelper.GenerateSlug(request.Slug);

        var slugExists = await db.Authors
            .AnyAsync(author => author.Slug == slug, ct);

        if (slugExists)
        {
            return Results.Conflict(ApiResponse<DomainError>.Fail(AuthorErrors.SlugAlreadyExists));
        }

        var author = new Author(
            displayName: request.DisplayName,
            slug: slug,
            bio: request.Bio,
            avatarAssetId: request.AvatarAssetId,
            isActive: request.IsActive);

        db.Authors.Add(author);
        
        auditLogWriter.Add(
            action: AuditAction.AuthorCreated,
            entityName: "Author",
            entityId: author.Id.ToString(),
            newValues: new
            {
                author.Id,
                author.DisplayName,
                author.Slug,
                author.IsActive
            });

        await db.SaveChangesAsync(ct);

        var response = new CreateAuthorResponse
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

        return Results.Created(
            $"/api/authors/{author.Id}",
            ApiResponse<CreateAuthorResponse>.Ok(response));
    }
}