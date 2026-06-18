using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Abstractions.RateLimiting;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.RateLimiting;
using ContentHub.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContentHub.Api.Features.Auth.Login;

public sealed class LoginEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.Login, Handle)
            .WithTags("Auth")
            .WithName("Login")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<LoginCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] LoginCommand request,
        ContentHubDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        IOptions<RateLimitOptions> rateLimitOptions,
        IPermissionService permissionService,
        IRateLimitService rateLimitService,
        RateLimitKeyBuilder rateLimitKeyBuilder,
        AuditLogWriter auditLogWriter,
        HttpContext httpContext,

        CancellationToken ct)
    {
        var normalizedValue = request.EmailOrUsername.ToUpperInvariant();
        var loginRateLimit = rateLimitOptions.Value.Login;
        var rateLimitWindow = TimeSpan.FromMinutes(loginRateLimit.WindowMinutes);
        var rateLimitKey = rateLimitKeyBuilder.BuildLoginKey(
            request.EmailOrUsername,
            httpContext.Connection.RemoteIpAddress?.ToString());

        var currentLimit = await rateLimitService.CheckAsync(
            rateLimitKey,
            loginRateLimit.MaxFailedAttempts,
            rateLimitWindow,
            ct);

        if (currentLimit.IsLimited)
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.LoginRateLimited),
                statusCode: StatusCodes.Status429TooManyRequests);
        }

        var user = await db.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user =>
                user.NormalizedEmail == normalizedValue ||
                user.NormalizedUsername == normalizedValue,
                ct);

        if (user is null)
        {
            await rateLimitService.IncrementAsync(
                rateLimitKey,
                loginRateLimit.MaxFailedAttempts,
                rateLimitWindow,
                ct);

            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.InvalidCredentials),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!user.IsActive)
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.UserDisabled),
                statusCode: StatusCodes.Status403Forbidden);
        }

        var passwordValid = passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            await rateLimitService.IncrementAsync(
                rateLimitKey,
                loginRateLimit.MaxFailedAttempts,
                rateLimitWindow,
                ct);

            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.InvalidCredentials),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        await rateLimitService.ResetAsync(rateLimitKey, ct);

        var roles = await permissionService.GetRolesAsync(user.Id, ct);

        var accessToken = jwtTokenGenerator.Generate(user, roles);

        var refreshToken = refreshTokenGenerator.Generate();
        var refreshTokenHash = refreshTokenGenerator.Hash(refreshToken);

        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(
            jwtOptions.Value.RefreshTokenExpirationDays);

        user.AddRefreshToken(
            tokenHash: refreshTokenHash,
            expiresAtUtc: refreshTokenExpiresAtUtc,
            userAgent: httpContext.Request.Headers.UserAgent.ToString(),
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        var session = user.AddSession(
            refreshTokenHash: refreshTokenHash,
            expiresAtUtc: refreshTokenExpiresAtUtc,
            userAgent: httpContext.Request.Headers.UserAgent.ToString(),
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        accessToken = jwtTokenGenerator.Generate(user, roles, session.Id);

        var oldValues = new
        {
            user.Id,
            user.LastLoginAtUtc
        };

        user.MarkLoggedIn();

        auditLogWriter.AddAnonymous(
            action: AuditAction.UserLoggedIn,
            entityName: "User",
            entityId: user.Id.ToString(),
            oldValues: oldValues,
            newValues: new
            {
                user.Id,
                user.LastLoginAtUtc,
                SessionId = session.Id
            });

        await db.SaveChangesAsync(ct);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes),
            User = new AuthUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                DisplayName = user.DisplayName
            }
        };

        return Results.Ok(ApiResponse<LoginResponse>.Ok(response));
    }
}
