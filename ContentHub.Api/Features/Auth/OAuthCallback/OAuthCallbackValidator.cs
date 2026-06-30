using ContentHub.Api.Features.Auth.Shared;
using FluentValidation;

namespace ContentHub.Api.Features.Auth.OAuthCallback;

public sealed class OAuthCallbackValidator : AbstractValidator<OAuthCallbackQuery>
{
    public OAuthCallbackValidator()
    {
        RuleFor(query => query.Provider)
            .NotEmpty()
            .WithMessage("OAuth provider is required.")
            .Must(OAuthProviderHelper.IsSupported)
            .WithMessage("OAuth provider must be google or github.");
    }
}
