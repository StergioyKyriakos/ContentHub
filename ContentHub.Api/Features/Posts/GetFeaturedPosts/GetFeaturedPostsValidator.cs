using FluentValidation;

namespace ContentHub.Api.Features.Posts.GetFeaturedPosts;

public sealed class GetFeaturedPostsQueryValidator : AbstractValidator<GetFeaturedPostsQuery>
{
    public GetFeaturedPostsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Page size for featured items must be between 1 and 50.");
    }
}