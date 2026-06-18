using System.Security.Claims;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.GetSessions;

public sealed class GetSessionsEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuthEndpoints.Sessions, Handle)
            .WithTags("Auth")
            .WithName("GetSessions")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle(
        [FromBody] GetSessionsQuery query,
        ICurrentUserProvider currentUser,
        ContentHubDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (currentUser.UserId is null)
        {
            return ResultsFactory.Unauthorized();
        }

        var currentSessionId = GetCurrentSessionId(httpContext);

        var sessions = await db.UserSessions
            .AsNoTracking()
            .Where(session =>
                session.UserId == currentUser.UserId.Value &&
                session.RevokedAtUtc == null &&
                session.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(session => session.CreatedAtUtc)
            .Select(session => new UserSessionResponse
            {
                Id = session.Id,
                CreatedAtUtc = session.CreatedAtUtc,
                ExpiresAtUtc = session.ExpiresAtUtc,
                UserAgent = session.UserAgent,
                IpAddress = session.IpAddress,
                IsCurrent = currentSessionId.HasValue && session.Id == currentSessionId.Value
            })
            .ToListAsync(ct);

        var response = new GetSessionsResponse
        {
            Sessions = sessions
        };

        return Results.Ok(ApiResponse<GetSessionsResponse>.Ok(response));
    }

    private static Guid? GetCurrentSessionId(HttpContext httpContext)
    {
        var value = httpContext.User.FindFirstValue("sessionId");

        return Guid.TryParse(value, out var sessionId)
            ? sessionId
            : null;
    }
}
