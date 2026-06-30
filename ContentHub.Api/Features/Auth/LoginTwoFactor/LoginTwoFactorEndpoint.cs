using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Login;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Authentication;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContentHub.Api.Features.Auth.LoginTwoFactor;

public sealed class LoginTwoFactorEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.LoginTwoFactor, Handle)
            .WithTags("Auth")
            .WithName("LoginTwoFactor")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [FromBody] LoginTwoFactorCommand command,
        IValidator<LoginTwoFactorCommand> validator,
        ContentHubDbContext db,
        ITwoFactorService twoFactorService,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        IPermissionService permissionService,
        AuditLogWriter auditLogWriter,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        var user = await db.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.Id == command.UserId, ct);

        if (user is null)
        {
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

        if (!user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            return ResultsFactory.BadRequest(
                "auth.two_factor_not_enabled",
                "Two-factor authentication is not enabled for this user.");
        }

        var valid = twoFactorService.VerifyCode(
            user.TwoFactorSecret,
            command.Code);

        if (!valid)
        {
            return Results.Json(
                ApiResponse<object>.Fail(ApiError.Create(
                    "auth.invalid_two_factor_code",
                    "Invalid two-factor code.")),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var roles = await permissionService.GetRolesAsync(user.Id, ct);

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

        var accessToken = jwtTokenGenerator.Generate(user, roles, session.Id);

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
                SessionId = session.Id,
                TwoFactorUsed = true
            });

        await db.SaveChangesAsync(ct);

        var response = new LoginTwoFactorResponse
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

        return Results.Ok(ApiResponse<LoginTwoFactorResponse>.Ok(response));
    }
}
