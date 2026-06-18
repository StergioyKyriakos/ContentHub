using System.Security.Claims;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.RevokeCurrentSession;

public sealed class RevokeCurrentSessionEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete(AuthEndpoints.CurrentSession, Handle)
            .WithTags("Auth")
            .WithName("RevokeCurrentSession")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle(
        [FromBody] RevokeCurrentSessionCommand command,
        ICurrentUserProvider currentUser,
        ContentHubDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return ResultsFactory.Unauthorized();
        }

        var sessionId = GetCurrentSessionId(httpContext);

        if (sessionId is null)
        {
            return Results.NotFound(ApiResponse<object>.Fail(AuthErrors.SessionNotFound));
        }

        var session = await db.UserSessions
            .FirstOrDefaultAsync(session =>
                session.Id == sessionId.Value &&
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

        var response = new RevokeCurrentSessionResponse
        {
            Id = session.Id,
            Revoked = true
        };

        return Results.Ok(ApiResponse<RevokeCurrentSessionResponse>.Ok(response));
    }

    private static Guid? GetCurrentSessionId(HttpContext httpContext)
    {
        var value = httpContext.User.FindFirstValue("sessionId");

        return Guid.TryParse(value, out var sessionId)
            ? sessionId
            : null;
    }
}
