using FluentValidation;

namespace ContentHub.Api.Features.Auth.VerifyEmail;

public sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(320)
            .WithMessage("Email must be 320 characters or fewer.");

        RuleFor(command => command.Token)
            .NotEmpty()
            .WithMessage("Verification token is required.")
            .MaximumLength(500)
            .WithMessage("Verification token is too long.");
    }
}
