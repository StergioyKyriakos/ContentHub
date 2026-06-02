using FluentValidation;

namespace ContentHub.Api.Features.Auth.RevokeSession;

public sealed class RevokeSessionValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Session id is required.");
    }
}
