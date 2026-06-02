using FluentValidation;

namespace ContentHub.Api.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty");
    }
}
