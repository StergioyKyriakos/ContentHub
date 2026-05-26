using FluentValidation;

namespace ContentHub.Api.Features.Categories.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Slug)
            .MaximumLength(180)
            .When(command => !string.IsNullOrWhiteSpace(command.Slug));

        RuleFor(command => command.Description)
            .MaximumLength(1000)
            .When(command => !string.IsNullOrWhiteSpace(command.Description));

        RuleFor(command => command.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}