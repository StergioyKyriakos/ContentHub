using FluentValidation;

namespace ContentHub.Api.Features.Posts.GetPublishedPosts;

public sealed class GetPublishedPostsQueryValidator : AbstractValidator<GetPublishedPostsQuery>
{
    public GetPublishedPostsQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("Search query cannot exceed 100 characters.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().When(x => x.CategoryId.HasValue)
            .WithMessage("Category ID cannot be an empty GUID.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().When(x => x.AuthorId.HasValue)
            .WithMessage("Author ID cannot be an empty GUID.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}