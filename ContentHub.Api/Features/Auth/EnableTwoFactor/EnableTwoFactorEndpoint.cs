using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Authentication;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContentHub.Api.Features.Auth.EnableTwoFactor;

public sealed class EnableTwoFactorEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.EnableTwoFactor, Handle)
            .WithTags("Auth")
            .WithName("EnableTwoFactor")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] EnableTwoFactorCommand command,
        IValidator<EnableTwoFactorCommand> validator,
        ICurrentUserProvider currentUserProvider,
        ITwoFactorService twoFactorService,
        IOptions<JwtOptions> jwtOptions,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        if (currentUserProvider.UserId is null)
        {
            return ResultsFactory.Unauthorized();
        }

        var user = await db.Users
            .FirstOrDefaultAsync(user => user.Id == currentUserProvider.UserId.Value, ct);

        if (user is null)
        {
            return Results.NotFound(ApiResponse<object>.Fail(AuthErrors.UserNotFound));
        }

        if (user.TwoFactorEnabled)
        {
            return ResultsFactory.BadRequest(
                "auth.two_factor_already_enabled",
                "Two-factor authentication is already enabled.");
        }

        var secret = twoFactorService.GenerateSecret();

        user.TwoFactorSecret = secret;
        user.TwoFactorEnabled = false;

        await db.SaveChangesAsync(ct);

        var response = new EnableTwoFactorResponse
        {
            Secret = secret,
            AuthenticatorUri = twoFactorService.BuildAuthenticatorUri(
                jwtOptions.Value.Issuer,
                user.Email,
                secret)
        };

        return Results.Ok(ApiResponse<EnableTwoFactorResponse>.Ok(response));
    }
}
