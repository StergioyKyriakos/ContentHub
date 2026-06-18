using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.ForgotPassword, Handle)
            .WithTags("Auth")
            .WithName("ForgotPassword")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<ForgotPasswordCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] ForgotPasswordCommand request,
        ContentHubDbContext db,
        ISecurityTokenGenerator securityTokenGenerator,
        IAuthEmailSender authEmailSender,
        HttpContext httpContext,
        ILogger<ForgotPasswordEndpoint> logger,
        CancellationToken ct)
    {
        var response = new ForgotPasswordResponse
        {
            Message = "If the account exists, a password reset link has been sent."
        };

        var normalizedEmail = request.Email.ToUpperInvariant();

        var user = await db.Users
            .Include(user => user.PasswordResetTokens)
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, ct);

        if (user is null || !user.IsActive)
        {
            return Results.Ok(ApiResponse<ForgotPasswordResponse>.Ok(response));
        }

        foreach (var existingToken in user.PasswordResetTokens.Where(token => token.IsActive))
        {
            existingToken.Revoke();
        }

        var token = securityTokenGenerator.Generate();
        var tokenHash = securityTokenGenerator.Hash(token);

        user.AddPasswordResetToken(
            tokenHash: tokenHash,
            expiresAtUtc: DateTime.UtcNow.AddHours(1),
            userAgent: httpContext.Request.Headers.UserAgent.ToString(),
            ipAddress: httpContext.Connection.RemoteIpAddress?.ToString());

        await db.SaveChangesAsync(ct);
        try
        {
            await authEmailSender.SendPasswordResetAsync(user, token, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Password reset email delivery failed for user {UserId}.", user.Id);
        }

        return Results.Ok(ApiResponse<ForgotPasswordResponse>.Ok(response));
    }
}
