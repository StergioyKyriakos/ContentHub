using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Search.Shared;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Infrastructure.Search.OpenSearch;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Features.Search.ReindexSearch;

public sealed class ReindexSearchEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(SearchEndpoints.Reindex, Handle)
            .WithTags("Search")
            .WithName("ReindexSearch")
            .RequireAuthorization(Policies.AdminOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] ReindexSearchCommand command,
        IValidator<ReindexSearchCommand> validator,
        OpenSearchIndex openSearchIndex,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var result = await openSearchIndex.ReindexAsync(ct);

            var response = new ReindexSearchResponse
            {
                Provider = result.Provider,
                OpenSearchEnabled = result.OpenSearchEnabled,
                PostsIndexed = result.PostsIndexed,
                DocumentsIndexed = result.DocumentsIndexed,
                Message = result.Message
            };

            return Results.Ok(ApiResponse<ReindexSearchResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            return Results.Json(
                ApiResponse<object>.Fail(ApiError.Create(
                    "search.reindex_failed",
                    $"Search reindex failed: {ex.Message}")),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
