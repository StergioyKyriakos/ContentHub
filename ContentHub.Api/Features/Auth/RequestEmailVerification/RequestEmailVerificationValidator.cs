using FluentValidation;

namespace ContentHub.Api.Features.Auth.RequestEmailVerification;

public sealed class RequestEmailVerificationValidator : AbstractValidator<RequestEmailVerificationCommand>
{
    public RequestEmailVerificationValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(320)
            .WithMessage("Email must be 320 characters or fewer.");
    }
}
