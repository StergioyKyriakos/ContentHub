using System.Security.Claims;
using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Extensions;
using ContentHub.Api.Features.Auth.Login;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Users;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Authentication;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContentHub.Api.Features.Auth.OAuthCallback;

public sealed class OAuthCallbackEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuthEndpoints.OAuthCallback, Handle)
            .WithTags("Auth")
            .WithName("OAuthCallback")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [AsParameters] OAuthCallbackQuery query,
        IValidator<OAuthCallbackQuery> validator,
        ContentHubDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        IPermissionService permissionService,
        AuditLogWriter auditLogWriter,
        IAuthenticationSchemeProvider schemeProvider,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        var scheme = OAuthProviderHelper.GetScheme(query.Provider);
        if (scheme is null)
        {
            return ResultsFactory.BadRequest(
                "auth.unsupported_oauth_provider",
                "OAuth provider must be google or github.");
        }

        var authenticationScheme = await schemeProvider.GetSchemeAsync(scheme);
        if (authenticationScheme is null)
        {
            return ResultsFactory.BadRequest(
                "auth.oauth_provider_not_configured",
                "OAuth provider is not configured.");
        }

        var externalResult = await httpContext.AuthenticateAsync(
            AuthenticationExtensions.ExternalAuthenticationScheme);

        if (!externalResult.Succeeded || externalResult.Principal is null)
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.ExternalLoginFailed),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var providerUserId = externalResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = externalResult.Principal.FindFirstValue(ClaimTypes.Email);
        var displayName = externalResult.Principal.FindFirstValue(ClaimTypes.Name) ??
                          externalResult.Principal.FindFirstValue("urn:github:login") ??
                          email;

        if (string.IsNullOrWhiteSpace(providerUserId))
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.ExternalLoginFailed),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(AuthErrors.ExternalEmailMissing));
        }

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var createdUser = false;
        var linkedExternalAccount = false;

        var externalLogin = await db.UserExternalLogins
            .Include(login => login.User)
            .FirstOrDefaultAsync(login =>
                    login.Provider == scheme &&
                    login.ProviderUserId == providerUserId,
                ct);

        var user = externalLogin?.User;

        if (user is null)
        {
            user = await db.Users
                .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, ct);

            if (user is null)
            {
                var username = await OAuthUsernameBuilder.CreateUniqueUsernameAsync(
                    db,
                    email,
                    scheme,
                    providerUserId,
                    ct);

                user = new User(
                    email: email,
                    username: username,
                    displayName: OAuthUsernameBuilder.TrimToMax(displayName ?? email, 100),
                    passwordHash: passwordHasher.Hash(Guid.CreateVersion7().ToString()));

                user.MarkEmailAsVerified();

                db.Users.Add(user);
                createdUser = true;

                auditLogWriter.AddAnonymous(
                    action: AuditAction.UserRegistered,
                    entityName: "User",
                    entityId: user.Id.ToString(),
                    newValues: new
                    {
                        user.Id,
                        user.Email,
                        user.Username,
                        user.DisplayName,
                        Provider = scheme
                    });
            }

            user.AddExternalLogin(
                provider: scheme,
                providerUserId: providerUserId,
                email: email,
                displayName: displayName);

            linkedExternalAccount = true;
        }

        if (!user.IsActive)
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.UserDisabled),
                statusCode: StatusCodes.Status403Forbidden);
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
                Provider = scheme,
                CreatedUser = createdUser,
                LinkedExternalAccount = linkedExternalAccount
            });

        await db.SaveChangesAsync(ct);

        await httpContext.SignOutAsync(AuthenticationExtensions.ExternalAuthenticationScheme);

        var response = new OAuthCallbackResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes),
            Provider = scheme,
            CreatedUser = createdUser,
            LinkedExternalAccount = linkedExternalAccount,
            User = new AuthUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                DisplayName = user.DisplayName
            }
        };

        return Results.Ok(ApiResponse<OAuthCallbackResponse>.Ok(response));
    }
}
