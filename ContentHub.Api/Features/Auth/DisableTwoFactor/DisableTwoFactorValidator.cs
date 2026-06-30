using FluentValidation;

namespace ContentHub.Api.Features.Auth.DisableTwoFactor;

public sealed class DisableTwoFactorValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty()
            .WithMessage("Two-factor code is required.")
            .Length(6)
            .WithMessage("Two-factor code must be 6 digits.")
            .Matches("^[0-9]{6}$")
            .WithMessage("Two-factor code must contain only digits.");
    }
}
