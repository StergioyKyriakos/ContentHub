using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using ContentHub.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.Logout;

public sealed class LogoutEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.Logout, Handle)
            .WithTags("Auth")
            .WithName("Logout")
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<LogoutCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] LogoutCommand request,
        ContentHubDbContext db,
        IRefreshTokenGenerator refreshTokenGenerator,
        CancellationToken ct)
    {
        var refreshTokenHash = refreshTokenGenerator.Hash(request.RefreshToken);

        var refreshToken = await db.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == refreshTokenHash, ct);

        if (refreshToken is not null && refreshToken.IsActive)
        {
            refreshToken.Revoke();

            var session = await db.UserSessions
                .FirstOrDefaultAsync(session =>
                    session.RefreshTokenHash == refreshTokenHash &&
                    session.RevokedAtUtc == null,
                    ct);

            session?.Revoke();

            await db.SaveChangesAsync(ct);
        }
        
        var response = new LogoutResponse();

        return Results.Ok(ApiResponse<LogoutResponse>.Ok(response));
    }
}
