using FluentValidation;

namespace ContentHub.Api.Features.Search.SearchPosts;

public sealed class SearchPostsQueryValidator : AbstractValidator<SearchPostsQuery>
{
    private static readonly string[] AllowedSortFields = ["relevance", "publishedAt", "createdAt", "title", "isFeatured"];
    private static readonly string[] AllowedDirections = ["asc", "desc"];

    public SearchPostsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100 entries.");

        RuleFor(x => x.Q)
            .MaximumLength(100).WithMessage("Search keyword phrase cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Q));

        RuleFor(x => x.CategoryId)
            .NotEmpty().When(x => x.CategoryId.HasValue).WithMessage("Category ID cannot be an empty GUID.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().When(x => x.AuthorId.HasValue).WithMessage("Author ID cannot be an empty GUID.");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue).WithMessage("Provided post status indicator is invalid.");

        RuleFor(x => x.SortBy)
            .Must(x => AllowedSortFields.Contains(x?.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Sorting field must match one of the following: {string.Join(", ", AllowedSortFields)}.");

        RuleFor(x => x.SortDirection)
            .Must(x => AllowedDirections.Contains(x?.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Sorting direction must be either 'asc' or 'desc'.");

        RuleFor(x => x.PublishedFrom)
            .LessThanOrEqualTo(x => x.PublishedTo!.Value)
            .When(x => x.PublishedFrom.HasValue && x.PublishedTo.HasValue)
            .WithMessage("The 'PublishedFrom' boundary cannot occur after the 'PublishedTo' boundary.");
    }
}
