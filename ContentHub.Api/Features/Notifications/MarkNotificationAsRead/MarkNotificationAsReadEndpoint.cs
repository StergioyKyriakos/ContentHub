using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Notifications.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Application.Common.Security;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Features.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPatch(NotificationEndpoints.MarkAsRead, Handle)
            .WithTags("Notifications")
            .WithName("MarkNotificationAsRead")
            .RequireAuthorization(Policies.AuthenticatedOnly);
    }

    private static async Task<IResult> Handle(
        [FromBody] MarkNotificationAsReadCommand command,
        IValidator<MarkNotificationAsReadCommand> validator,
        ICurrentUserProvider currentUserProvider,
        ContentHubDbContext db,
        CancellationToken ct)
    {
        if (currentUserProvider.UserId is null)
        {
            return Results.Json(
                ApiResponse<DomainError>.Fail(NotificationErrors.UserNotAuthenticated),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var notification = await db.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == command.Id &&
                n.UserId == currentUserProvider.UserId.Value,
                ct);

        if (notification is null)
        {
            return Results.NotFound(ApiResponse<DomainError>.Fail(NotificationErrors.NotFound));
        }

        notification.MarkAsRead();

        await db.SaveChangesAsync(ct);

        var response = new MarkNotificationAsReadResponse();

        return Results.Ok(ApiResponse<MarkNotificationAsReadResponse>.Ok(response));
    }
}