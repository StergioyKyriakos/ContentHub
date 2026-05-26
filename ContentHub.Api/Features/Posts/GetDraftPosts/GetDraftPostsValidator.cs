using FluentValidation;

namespace ContentHub.Api.Features.Posts.GetDraftPosts;

public sealed class GetDraftPostsQueryValidator : AbstractValidator<GetDraftPostsQuery>
{
    public GetDraftPostsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}