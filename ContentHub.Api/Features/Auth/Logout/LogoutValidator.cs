using FluentValidation;

namespace ContentHub.Api.Features.Auth.Logout;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty().WithMessage("RefreshToken is required");
    }
}