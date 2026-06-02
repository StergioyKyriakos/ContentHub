using FluentValidation;

namespace ContentHub.Api.Features.Auth.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(320)
            .WithMessage("Email must be 320 characters or fewer.");

        RuleFor(command => command.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters.")
            .MaximumLength(100)
            .WithMessage("Username must be 100 characters or fewer.");

        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required.")
            .MaximumLength(150)
            .WithMessage("Display name must be 150 characters or fewer.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100)
            .WithMessage("Password must be 100 characters or fewer.");
    }
}
