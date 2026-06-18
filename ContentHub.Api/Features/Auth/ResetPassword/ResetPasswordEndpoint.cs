using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.ResetPassword;

public sealed class ResetPasswordEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.ResetPassword, Handle)
            .WithTags("Auth")
            .WithName("ResetPassword")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<ResetPasswordCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] ResetPasswordCommand request,
        ContentHubDbContext db,
        ISecurityTokenGenerator securityTokenGenerator,
        IPasswordHasher passwordHasher,
        CancellationToken ct)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();
        var tokenHash = securityTokenGenerator.Hash(request.Token);

        var resetToken = await db.PasswordResetTokens
            .Include(token => token.User)
            .ThenInclude(user => user.RefreshTokens)
            .Include(token => token.User)
            .ThenInclude(user => user.Sessions)
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash &&
                token.User.NormalizedEmail == normalizedEmail,
                ct);

        if (resetToken is null || !resetToken.IsActive)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(AuthErrors.InvalidPasswordResetToken));
        }

        var user = resetToken.User;

        if (!user.IsActive)
        {
            return Results.Json(
                ApiResponse<object>.Fail(AuthErrors.UserDisabled),
                statusCode: StatusCodes.Status403Forbidden);
        }

        resetToken.Consume();
        user.ChangePasswordHash(passwordHasher.Hash(request.NewPassword));

        foreach (var refreshToken in user.RefreshTokens.Where(token => token.IsActive))
        {
            refreshToken.Revoke();
        }

        foreach (var session in user.Sessions.Where(session => session.IsActive))
        {
            session.Revoke();
        }

        await db.SaveChangesAsync(ct);

        var response = new ResetPasswordResponse
        {
            Message = "Password reset successfully."
        };

        return Results.Ok(ApiResponse<ResetPasswordResponse>.Ok(response));
    }
}
