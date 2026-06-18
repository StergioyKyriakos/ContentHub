using ContentHub.Api.Common.Auditing;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Entities.Users;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.Register;

public sealed class RegisterEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.Register, Handle)
            .WithTags("Auth")
            .WithName("Register")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<RegisterCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] RegisterCommand request,
        ContentHubDbContext db,
        IPasswordHasher passwordHasher,
        ISecurityTokenGenerator securityTokenGenerator,
        IAuthEmailSender authEmailSender,
        AuditLogWriter auditLogWriter,
        HttpContext httpContext,
        ILogger<RegisterEndpoint> logger,
        CancellationToken ct)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();
        var normalizedUsername = request.Username.ToUpperInvariant();

        var emailExists = await db.Users
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, ct);

        if (emailExists)
        {
            return Results.Conflict(ApiResponse<DomainError>.Fail(AuthErrors.EmailAlreadyExists));
        }

        var usernameExists = await db.Users
            .AnyAsync(user => user.NormalizedUsername == normalizedUsername, ct);

        if (usernameExists)
        {
            return Results.Conflict(ApiResponse<DomainError>.Fail(AuthErrors.UsernameAlreadyExists));
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = new User(
            email: request.Email,
            username: request.Username,
            passwordHash: passwordHash,
            displayName: request.DisplayName);
        
        
        db.Users.Add(user);

        var verificationToken = securityTokenGenerator.Generate();

        user.AddEmailVerificationToken(
            tokenHash: securityTokenGenerator.Hash(verificationToken),
            expiresAtUtc: DateTime.UtcNow.AddHours(24),
            userAgent: httpContext.Request.Headers.UserAgent.ToString(),
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

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
                user.EmailVerified,
                user.Status
            });

        await db.SaveChangesAsync(ct);
        try
        {
            await authEmailSender.SendEmailVerificationAsync(user, verificationToken, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Email verification delivery failed for user {UserId}.", user.Id);

            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.EmailDeliveryFailed),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var response = new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            DisplayName = user.DisplayName
        };

        return Results.Ok(ApiResponse<RegisterResponse>.Ok(response));
    }
}
