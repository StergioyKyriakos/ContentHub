using FluentValidation;

namespace ContentHub.Api.Features.Auth.LoginTwoFactor;

public sealed class LoginTwoFactorValidator : AbstractValidator<LoginTwoFactorCommand>
{
    public LoginTwoFactorValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("User id is required.");

        RuleFor(command => command.Code)
            .NotEmpty()
            .WithMessage("Two-factor code is required.")
            .Length(6)
            .WithMessage("Two-factor code must be 6 digits.")
            .Matches("^[0-9]{6}$")
            .WithMessage("Two-factor code must contain only digits.");
    }
}
