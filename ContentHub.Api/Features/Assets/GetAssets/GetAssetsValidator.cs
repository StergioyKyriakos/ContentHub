using FluentValidation;

namespace ContentHub.Api.Features.Assets.GetAssets;

public sealed class GetAssetsQueryValidator : AbstractValidator<GetAssetsQuery>
{
    public GetAssetsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100 items.");

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("Search query cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Provided asset type filter is invalid.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Visibility)
            .IsInEnum().WithMessage("Provided visibility filter is invalid.")
            .When(x => x.Visibility.HasValue);
    }
}