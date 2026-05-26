using FluentValidation;

namespace ContentHub.Api.Features.Authors.UpdateAuthor;

public class UpdateAuthorValidator : AbstractValidator<UpdateAuthorCommand>
{
    public UpdateAuthorValidator()
    {
        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Slug)
            .MaximumLength(180)
            .When(command => !string.IsNullOrWhiteSpace(command.Slug));

        RuleFor(command => command.Bio)
            .MaximumLength(3000)
            .When(command => !string.IsNullOrWhiteSpace(command.Bio));
    }
}