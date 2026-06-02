using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.VerifyEmail;

public sealed class VerifyEmailEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.VerifyEmail, Handle)
            .WithTags("Auth")
            .WithName("VerifyEmail")
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<VerifyEmailCommand>>();
    }

    private static async Task<IResult> Handle(
        [FromBody] VerifyEmailCommand request,
        ContentHubDbContext db,
        ISecurityTokenGenerator securityTokenGenerator,
        CancellationToken ct)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();
        var tokenHash = securityTokenGenerator.Hash(request.Token);

        var verificationToken = await db.EmailVerificationTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token =>
                token.TokenHash == tokenHash &&
                token.User.NormalizedEmail == normalizedEmail,
                ct);

        if (verificationToken is null || !verificationToken.IsActive)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(AuthErrors.InvalidEmailVerificationToken));
        }

        verificationToken.Consume();
        verificationToken.User.MarkEmailAsVerified();

        await db.SaveChangesAsync(ct);

        var response = new VerifyEmailResponse
        {
            Message = "Email address verified."
        };

        return Results.Ok(ApiResponse<VerifyEmailResponse>.Ok(response));
    }
}
