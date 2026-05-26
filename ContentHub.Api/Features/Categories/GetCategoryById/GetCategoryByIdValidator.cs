using FluentValidation;

namespace ContentHub.Api.Features.Categories.GetCategoryById;

public class GetCategoryByIdValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdValidator()
    {
        RuleFor(x => x.id).NotEmpty().WithMessage("Id cannot be empty");
    }
}