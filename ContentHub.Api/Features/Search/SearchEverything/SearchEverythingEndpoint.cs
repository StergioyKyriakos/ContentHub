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
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
        var search = query.Q?.Trim();
        var hasSearch = !string.IsNullOrWhiteSpace(search);

        var postsQuery = db.Posts.AsNoTracking()
            .Where(post => post.Status == PostStatus.Published);

        if (hasSearch)
        {
            postsQuery = postsQuery.Where(post => post.SearchVector.Matches(
                EF.Functions.WebSearchToTsQuery("english", search!)));
        }

        var assetsQuery = db.Assets.AsNoTracking();
        
        var includeAssets = isAuthenticated;
        if (includeAssets && hasSearch)
        {
            assetsQuery = assetsQuery.Where(asset => asset.SearchVector.Matches(
                EF.Functions.WebSearchToTsQuery("simple", search!)));
        }

        var totalPosts = await postsQuery.CountAsync(ct);
        var totalAssets = includeAssets ? await assetsQuery.CountAsync(ct) : 0;
        var totalItems = totalPosts + totalAssets;

        var upperLimit = query.Page * query.PageSize;

        var postRows = hasSearch
            ? await postsQuery
                .OrderByDescending(post => post.SearchVector.Rank(EF.Functions.WebSearchToTsQuery("english", search!)))
                .ThenByDescending(post => post.PublishedAtUtc)
                .Take(upperLimit)
                .Select(post => new
                {
                    post.Id,
                    post.Title,
                    post.Summary,
                    post.Slug,
                    CreatedAtUtc = post.PublishedAtUtc ?? post.CreatedAtUtc,
                    Rank = post.SearchVector.Rank(EF.Functions.WebSearchToTsQuery("english", search!))
                })
                .ToListAsync(ct)
            : await postsQuery
                .OrderByDescending(post => post.PublishedAtUtc)
                .Take(upperLimit)
                .Select(post => new
                {
                    post.Id,
                    post.Title,
                    post.Summary,
                    post.Slug,
                    CreatedAtUtc = post.PublishedAtUtc ?? post.CreatedAtUtc,
                    Rank = 0.0f
                })
                .ToListAsync(ct);

        var postResults = postRows
            .Select(post => new RankedSearchItem(
                new SearchEverythingItemDto
                {
                    Type = SearchableContentType.Post,
                    Id = post.Id,
                    Title = post.Title,
                    Description = post.Summary,
                    Slug = post.Slug,
                    Url = $"/api/public/posts/{post.Slug}",
                    CreatedAtUtc = post.CreatedAtUtc
                },
                post.Rank))
            .ToList();

        var assetResults = new List<RankedSearchItem>();
        if (includeAssets)
        {
            var assets = hasSearch
                ? await assetsQuery
                    .OrderByDescending(asset => asset.SearchVector.Rank(EF.Functions.WebSearchToTsQuery("simple", search!)))
                    .ThenByDescending(asset => asset.CreatedAtUtc)
                    .Take(upperLimit)
                    .Select(asset => new
                    {
                        asset.Id,
                        asset.OriginalFileName,
                        asset.ContentType,
                        asset.StoragePath,
                        asset.Provider,
                        asset.CreatedAtUtc,
                        Rank = asset.SearchVector.Rank(EF.Functions.WebSearchToTsQuery("simple", search!))
                    })
                    .ToListAsync(ct)
                : await assetsQuery
                    .OrderByDescending(asset => asset.CreatedAtUtc)
                    .Take(upperLimit)
                    .Select(asset => new
                    {
                        asset.Id,
                        asset.OriginalFileName,
                        asset.ContentType,
                        asset.StoragePath,
                        asset.Provider,
                        asset.CreatedAtUtc,
                        Rank = 0.0f
                    })
                    .ToListAsync(ct);

            assetResults = assets
                .Select(asset => new RankedSearchItem(
                    new SearchEverythingItemDto
                    {
                        Type = SearchableContentType.Asset,
                        Id = asset.Id,
                        Title = asset.OriginalFileName,
                        Description = asset.ContentType,
                        Slug = null,
                        Url = fileUrlResolver.ResolveUrl(asset.StoragePath, asset.Provider),
                        CreatedAtUtc = asset.CreatedAtUtc
                    },
                    asset.Rank))
                .ToList();
        }

        var items = postResults
            .Concat(assetResults)
            .OrderByDescending(result => result.Rank)
            .ThenByDescending(result => result.Item.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(result => result.Item)
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

    private sealed record RankedSearchItem(
        SearchEverythingItemDto Item,
        float Rank);
}
