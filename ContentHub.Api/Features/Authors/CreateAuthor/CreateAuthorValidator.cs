using FluentValidation;

namespace ContentHub.Api.Features.Authors.CreateAuthor;

public class CreateAuthorValidator : AbstractValidator<CreateAuthorCommand>
{
    public CreateAuthorValidator()
    {
        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .WithMessage("Author display name is required.")
            .MaximumLength(150)
            .WithMessage("Author display name must be 150 characters or fewer.");

        RuleFor(command => command.Slug)
            .MaximumLength(180)
            .WithMessage("Author slug must be 180 characters or fewer.")
            .When(command => !string.IsNullOrWhiteSpace(command.Slug));

        RuleFor(command => command.Bio)
            .MaximumLength(3000)
            .WithMessage("Author bio must be 3000 characters or fewer.")
            .When(command => !string.IsNullOrWhiteSpace(command.Bio));
    }
}
