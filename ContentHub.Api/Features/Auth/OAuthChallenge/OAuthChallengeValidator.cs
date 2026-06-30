using ContentHub.Api.Features.Auth.Shared;
using FluentValidation;

namespace ContentHub.Api.Features.Auth.OAuthChallenge;

public sealed class OAuthChallengeValidator : AbstractValidator<OAuthChallengeQuery>
{
    public OAuthChallengeValidator()
    {
        RuleFor(query => query.Provider)
            .NotEmpty()
            .WithMessage("OAuth provider is required.")
            .Must(OAuthProviderHelper.IsSupported)
            .WithMessage("OAuth provider must be google or github.");
    }
}
