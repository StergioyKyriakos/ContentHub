using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Search.Shared;
using ContentHub.Application.Abstractions.Storage;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Dtos.Search;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Search.SearchEverything;

public sealed class SearchEverythingEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(SearchEndpoints.SearchEverything, Handle)
            .WithTags("Search")
            .WithName("SearchEverything")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromBody] SearchEverythingQuery query,
        IValidator<SearchEverythingQuery> validator,
        HttpContext httpContext,
        ContentHubDbContext db,
        IFileUrlResolver fileUrlResolver,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
        var search = query.Q?.Trim();
        var pattern = !string.IsNullOrWhiteSpace(search) ? $"%{search}%" : null;

        var postsQuery = db.Posts.AsNoTracking()
            .Where(post => post.Status == PostStatus.Published);

        if (pattern is not null)
        {
            postsQuery = postsQuery.Where(post =>
                EF.Functions.ILike(post.Title, pattern) ||
                EF.Functions.ILike(post.Slug, pattern) ||
                EF.Functions.ILike(post.Summary ?? string.Empty, pattern) ||
                EF.Functions.ILike(post.Content, pattern));
        }

        var assetsQuery = db.Assets.AsNoTracking();
        
        var includeAssets = isAuthenticated;
        if (includeAssets && pattern is not null)
        {
            assetsQuery = assetsQuery.Where(asset =>
                EF.Functions.ILike(asset.FileName, pattern) ||
                EF.Functions.ILike(asset.OriginalFileName, pattern) ||
                EF.Functions.ILike(asset.ContentType, pattern) ||
                EF.Functions.ILike(asset.StoragePath, pattern));
        }

        var totalPosts = await postsQuery.CountAsync(ct);
        var totalAssets = includeAssets ? await assetsQuery.CountAsync(ct) : 0;
        var totalItems = totalPosts + totalAssets;

        var upperLimit = query.Page * query.PageSize;

        var postResults = await postsQuery
            .OrderByDescending(post => post.PublishedAtUtc)
            .Take(upperLimit) 
            .Select(post => new SearchEverythingItemDto
            {
                Type = SearchableContentType.Post,
                Id = post.Id,
                Title = post.Title,
                Description = post.Summary,
                Slug = post.Slug,
                Url = $"/api/public/posts/{post.Slug}",
                CreatedAtUtc = post.PublishedAtUtc ?? post.CreatedAtUtc
            })
            .ToListAsync(ct);

        var assetResults = new List<SearchEverythingItemDto>();
        if (includeAssets)
        {
            var assets = await assetsQuery
                .OrderByDescending(asset => asset.CreatedAtUtc)
                .Take(upperLimit)
                .Select(asset => new
                {
                    asset.Id,
                    asset.OriginalFileName,
                    asset.ContentType,
                    asset.StoragePath,
                    asset.CreatedAtUtc
                })
                .ToListAsync(ct);

            assetResults = assets
                .Select(asset => new SearchEverythingItemDto
                {
                    Type = SearchableContentType.Asset,
                    Id = asset.Id,
                    Title = asset.OriginalFileName,
                    Description = asset.ContentType,
                    Slug = null,
                    Url = fileUrlResolver.ResolveUrl(asset.StoragePath),
                    CreatedAtUtc = asset.CreatedAtUtc
                })
                .ToList();
        }

        var items = postResults
            .Concat(assetResults)
            .OrderByDescending(result => result.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var response = new SearchEverythingResponse
        {
            Results = PagedResponse<SearchEverythingItemDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems)
        };

        return Results.Ok(ApiResponse<SearchEverythingResponse>.Ok(response));
    }
}
