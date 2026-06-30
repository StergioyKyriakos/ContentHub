using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Auth.ConfirmTwoFactor;

public sealed class ConfirmTwoFactorEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(AuthEndpoints.ConfirmTwoFactor, Handle)
            .WithTags("Auth")
            .WithName("ConfirmTwoFactor")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] ConfirmTwoFactorCommand command,
        IValidator<ConfirmTwoFactorCommand> validator,
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

        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            return ResultsFactory.BadRequest(
                "auth.two_factor_not_started",
                "Two-factor setup has not been started.");
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

        user.TwoFactorEnabled = true;

        await db.SaveChangesAsync(ct);

        var response = new ConfirmTwoFactorResponse
        {
            Enabled = true,
            Message = "Two-factor authentication enabled."
        };

        return Results.Ok(ApiResponse<ConfirmTwoFactorResponse>.Ok(response));
    }
}