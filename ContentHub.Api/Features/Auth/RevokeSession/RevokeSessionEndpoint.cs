using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.RevokeSession;

public sealed class RevokeSessionEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(AuthEndpoints.SessionById, Handle)
            .WithTags("Auth")
            .WithName("RevokeSession")
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<RevokeSessionCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] RevokeSessionCommand command,
        ICurrentUserProvider currentUser,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return Results.Unauthorized();
        }

        var session = await db.UserSessions
            .FirstOrDefaultAsync(session =>
                session.Id == command.Id &&
                session.UserId == currentUser.UserId.Value,
                ct);

        if (session is null)
        {
            return Results.NotFound(ApiResponse<object>.Fail(AuthErrors.SessionNotFound));
        }

        session.Revoke();

        var refreshToken = await db.RefreshTokens
            .FirstOrDefaultAsync(token =>
                token.TokenHash == session.RefreshTokenHash &&
                token.UserId == currentUser.UserId.Value &&
                token.RevokedAtUtc == null,
                ct);

        refreshToken?.Revoke();

        await db.SaveChangesAsync(ct);

        var response = new RevokeSessionResponse
        {
            Id = session.Id,
            Revoked = true
        };

        return Results.Ok(ApiResponse<RevokeSessionResponse>.Ok(response));
    }
}
