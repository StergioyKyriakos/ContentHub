using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.DisableTwoFactor;

public sealed class DisableTwoFactorEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.DisableTwoFactor, Handle)
            .WithTags("Auth")
            .WithName("DisableTwoFactor")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] DisableTwoFactorCommand command,
        IValidator<DisableTwoFactorCommand> validator,
        ICurrentUserProvider currentUserProvider,
        ITwoFactorService twoFactorService,
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

        if (!user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            return ResultsFactory.BadRequest(
                "auth.two_factor_not_enabled",
                "Two-factor authentication is not enabled.");
        }

        var valid = twoFactorService.VerifyCode(
            user.TwoFactorSecret,
            command.Code);

        if (!valid)
        {
            return ResultsFactory.BadRequest(
                "auth.invalid_two_factor_code",
                "Invalid two-factor code.");
        }

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;

        await db.SaveChangesAsync(ct);

        var response = new DisableTwoFactorResponse
        {
            Enabled = false,
            Message = "Two-factor authentication disabled."
        };

        return Results.Ok(ApiResponse<DisableTwoFactorResponse>.Ok(response));
    }
}