using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuthEndpoints.GetCurrentUser, Handle)
            .WithTags("Auth")
            .WithName("GetCurrentUser")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle(
        ICurrentUserProvider currentUserProvider,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        if (!currentUserProvider.IsAuthenticated || currentUserProvider.UserId is null)
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.InvalidCredentials),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == currentUserProvider.UserId.Value, ct);

        if (user is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(AuthErrors.UserNotFound));
        }

        var response = new GetCurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Status = user.Status.ToString()
        };

        return Results.Ok(ApiResponse<GetCurrentUserResponse>.Ok(response));
    }
}