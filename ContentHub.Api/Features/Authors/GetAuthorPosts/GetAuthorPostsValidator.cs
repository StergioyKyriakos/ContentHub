using FluentValidation;

namespace ContentHub.Api.Features.Authors.GetAuthorPosts;

public class GetAuthorPostsValidator : AbstractValidator<GetAuthorPostsQuery>
{
    public GetAuthorPostsValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
    }
}