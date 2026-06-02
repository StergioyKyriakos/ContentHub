using FluentValidation;

namespace ContentHub.Api.Features.Auth.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.EmailOrUsername)
            .NotEmpty()
            .WithMessage("Email or username is required.")
            .MaximumLength(320)
            .WithMessage("Email or username must be 320 characters or fewer.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MaximumLength(100)
            .WithMessage("Password must be 100 characters or fewer.");
    }
}
