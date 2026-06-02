using FluentValidation;

namespace ContentHub.Api.Features.Auth.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
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
            .WithMessage("Password reset token is required.")
            .MaximumLength(500)
            .WithMessage("Password reset token is too long.");

        RuleFor(command => command.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters.")
            .MaximumLength(100)
            .WithMessage("New password must be 100 characters or fewer.");
    }
}
