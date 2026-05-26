using FluentValidation;

namespace ContentHub.Api.Features.Categories.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
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