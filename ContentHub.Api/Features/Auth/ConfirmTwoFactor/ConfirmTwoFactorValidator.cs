using FluentValidation;

namespace ContentHub.Api.Features.Auth.ConfirmTwoFactor;

public sealed class ConfirmTwoFactorValidator : AbstractValidator<ConfirmTwoFactorCommand>
{
    public ConfirmTwoFactorValidator()
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
