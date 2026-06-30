using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Features.Auth.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;

namespace ContentHub.Api.Features.Auth.OAuthChallenge;

public sealed class OAuthChallengeEndpoint : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(AuthEndpoints.OAuthChallenge, Handle)
            .WithTags("Auth")
            .WithName("OAuthChallenge")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        [AsParameters] OAuthChallengeQuery query,
        IValidator<OAuthChallengeQuery> validator,
        IAuthenticationSchemeProvider schemeProvider,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return ResultsFactory.ValidationProblem(validationResult.ToDictionary());
        }

        var scheme = OAuthProviderHelper.GetScheme(query.Provider);
        if (scheme is null)
        {
            return ResultsFactory.BadRequest(
                "auth.unsupported_oauth_provider",
                "OAuth provider must be google or github.");
        }

        var authenticationScheme = await schemeProvider.GetSchemeAsync(scheme);
        if (authenticationScheme is null)
        {
            return ResultsFactory.BadRequest(
                "auth.oauth_provider_not_configured",
                "OAuth provider is not configured.");
        }

        var provider = query.Provider.Trim().ToLowerInvariant();
        var properties = new AuthenticationProperties
        {
            RedirectUri = $"/api/auth/oauth/{provider}/callback"
        };

        return Results.Challenge(properties, new[] { scheme });
    }
}
