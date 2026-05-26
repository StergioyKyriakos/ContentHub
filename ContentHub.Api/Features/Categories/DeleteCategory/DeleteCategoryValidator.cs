using FluentValidation;

namespace ContentHub.Api.Features.Categories.DeleteCategory;

public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}