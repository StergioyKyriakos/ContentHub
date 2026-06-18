using FluentValidation;

namespace ContentHub.Api.Features.Search.SearchAssets;

public sealed class SearchAssetsQueryValidator : AbstractValidator<SearchAssetsQuery>
{
    private static readonly string[] AllowedSortFields = ["relevance", "createdat", "filename", "originalfilename", "size", "type"];
    private static readonly string[] AllowedDirections = ["asc", "desc"];

    public SearchAssetsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100 entries.");

        RuleFor(x => x.Q)
            .MaximumLength(100).WithMessage("Search query parameter string cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Q));

        RuleFor(x => x.ContentType)
            .MaximumLength(100).WithMessage("ContentType string filter cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContentType));

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Provided asset type filter value is invalid.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Visibility)
            .IsInEnum().WithMessage("Provided visibility filter value is invalid.")
            .When(x => x.Visibility.HasValue);

        RuleFor(x => x.SortBy)
            .Must(x => AllowedSortFields.Contains(x?.Trim().ToLowerInvariant()))
            .WithMessage($"Sort field must match one of the following variations: {string.Join(", ", AllowedSortFields)}.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

        RuleFor(x => x.SortDirection)
            .Must(x => AllowedDirections.Contains(x?.Trim().ToLowerInvariant()))
            .WithMessage("Sort direction parameter can only be configured as 'asc' or 'desc'.")
            .When(x => !string.IsNullOrWhiteSpace(x.SortDirection));
    }
}
