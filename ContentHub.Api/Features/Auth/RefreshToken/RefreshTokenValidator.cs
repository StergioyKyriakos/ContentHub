using FluentValidation;

namespace ContentHub.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}
