using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.RevokeAllSessions;

public sealed class RevokeAllSessionsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(AuthEndpoints.Sessions, Handle)
            .WithTags("Auth")
            .WithName("RevokeAllSessions")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle(
        [FromBody] RevokeAllSessionsCommand command,
        ICurrentUserProvider currentUser,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return ResultsFactory.Unauthorized();
        }

        var sessions = await db.UserSessions
            .Where(session =>
                session.UserId == currentUser.UserId.Value &&
                session.RevokedAtUtc == null &&
                session.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(ct);

        var refreshTokenHashes = sessions
            .Select(session => session.RefreshTokenHash)
            .ToArray();

        var refreshTokens = await db.RefreshTokens
            .Where(token =>
                token.UserId == currentUser.UserId.Value &&
                refreshTokenHashes.Contains(token.TokenHash) &&
                token.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.Revoke();
        }

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.Revoke();
        }

        await db.SaveChangesAsync(ct);

        var response = new RevokeAllSessionsResponse
        {
            RevokedCount = sessions.Count
        };

        return Results.Ok(ApiResponse<RevokeAllSessionsResponse>.Ok(response));
    }
}
