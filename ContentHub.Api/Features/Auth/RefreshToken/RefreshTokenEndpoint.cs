using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContentHub.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.RefreshToken, Handle)
            .WithTags("Auth")
            .WithName("RefreshToken")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<RefreshTokenCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] RefreshTokenCommand request,
        ContentHubDbContext db,
        IRefreshTokenGenerator refreshTokenGenerator,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        IPermissionService permissionService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var refreshTokenHash = refreshTokenGenerator.Hash(request.RefreshToken);

        var existingRefreshToken = await db.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .FirstOrDefaultAsync(refreshToken =>
                refreshToken.TokenHash == refreshTokenHash,
                ct);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive)
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.InvalidRefreshToken),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var user = existingRefreshToken.User;

        if (!user.IsActive)
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.UserDisabled),
                statusCode: StatusCodes.Status403Forbidden);
        }

        var newRefreshToken = refreshTokenGenerator.Generate();
        var newRefreshTokenHash = refreshTokenGenerator.Hash(newRefreshToken);
        var newRefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays);

        existingRefreshToken.Revoke(newRefreshTokenHash);

        user.AddRefreshToken(
            tokenHash: newRefreshTokenHash,
            expiresAtUtc: newRefreshTokenExpiresAtUtc,
            userAgent: httpContext.Request.Headers.UserAgent.ToString(),
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        var session = await db.UserSessions
            .FirstOrDefaultAsync(session =>
                session.UserId == user.Id &&
                session.RefreshTokenHash == refreshTokenHash &&
                session.RevokedAtUtc == null,
                ct);

        if (session is null)
        {
            session = user.AddSession(
                refreshTokenHash: newRefreshTokenHash,
                expiresAtUtc: newRefreshTokenExpiresAtUtc,
                userAgent: httpContext.Request.Headers.UserAgent.ToString(),
                ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());
        }
        else
        {
            session.RotateRefreshToken(
                refreshTokenHash: newRefreshTokenHash,
                expiresAtUtc: newRefreshTokenExpiresAtUtc,
                userAgent: httpContext.Request.Headers.UserAgent.ToString(),
                ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());
        }

        var roles = await permissionService.GetRolesAsync(user.Id, ct);

        var accessToken = jwtTokenGenerator.Generate(user, roles, session.Id);

        await db.SaveChangesAsync(ct);

        var response = new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes)
        };

        return Results.Ok(ApiResponse<RefreshTokenResponse>.Ok(response));
    }
}
