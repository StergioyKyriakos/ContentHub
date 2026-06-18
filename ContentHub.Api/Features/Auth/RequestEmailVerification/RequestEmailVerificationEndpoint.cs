using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.RequestEmailVerification;

public sealed class RequestEmailVerificationEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.RequestEmailVerification, Handle)
            .WithTags("Auth")
            .WithName("RequestEmailVerification")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<RequestEmailVerificationCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] RequestEmailVerificationCommand request,
        ContentHubDbContext db,
        ISecurityTokenGenerator securityTokenGenerator,
        IAuthEmailSender authEmailSender,
        HttpContext httpContext,
        ILogger<RequestEmailVerificationEndpoint> logger,
        CancellationToken ct)
    {
        var response = new RequestEmailVerificationResponse
        {
            Message = "If the account exists and needs verification, an email verification link has been sent."
        };

        var normalizedEmail = request.Email.ToUpperInvariant();

        var user = await db.Users
            .Include(user => user.EmailVerificationTokens)
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, ct);

        if (user is null || user.EmailVerified || !user.IsActive)
        {
            return Results.Ok(ApiResponse<RequestEmailVerificationResponse>.Ok(response));
        }

        foreach (var existingToken in user.EmailVerificationTokens.Where(token => token.IsActive))
        {
            existingToken.Revoke();
        }

        var token = securityTokenGenerator.Generate();
        var tokenHash = securityTokenGenerator.Hash(token);

        user.AddEmailVerificationToken(
            tokenHash: tokenHash,
            expiresAtUtc: DateTime.UtcNow.AddHours(24),
            userAgent: httpContext.Request.Headers.UserAgent.ToString(),
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        await db.SaveChangesAsync(ct);
        try
        {
            await authEmailSender.SendEmailVerificationAsync(user, token, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Email verification delivery failed for user {UserId}.", user.Id);
        }

        return Results.Ok(ApiResponse<RequestEmailVerificationResponse>.Ok(response));
    }
}
