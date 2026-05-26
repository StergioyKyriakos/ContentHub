using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Common.Filters;
using ContentHub.Api.Features.Auth.Shared;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using ContentHub.Data.Entities.Users;
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

        await db.SaveChangesAsync(ct);

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