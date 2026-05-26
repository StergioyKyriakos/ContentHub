using FluentValidation;

namespace ContentHub.Api.Features.Search.SearchEverything;

public class SearchEverythingValidator : AbstractValidator<SearchEverythingQuery>
{
    public SearchEverythingValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100 entries.");

        RuleFor(x => x.Q)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Q));
    }
}