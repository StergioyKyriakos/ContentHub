using FluentValidation;

namespace ContentHub.Api.Features.Categories.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(150)
            .WithMessage("Category name must be 150 characters or fewer.");

        RuleFor(command => command.Slug)
            .MaximumLength(180)
            .WithMessage("Category slug must be 180 characters or fewer.")
            .When(command => !string.IsNullOrWhiteSpace(command.Slug));

        RuleFor(command => command.Description)
            .MaximumLength(1000)
            .WithMessage("Category description must be 1000 characters or fewer.")
            .When(command => !string.IsNullOrWhiteSpace(command.Description));

        RuleFor(command => command.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be 0 or greater.");
    }
}
