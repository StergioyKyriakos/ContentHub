using FluentValidation;

namespace ContentHub.Api.Features.Posts.GetPostById;

public sealed class GetPostByIdQueryValidator : AbstractValidator<GetPostByIdQuery>
{
    public GetPostByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Post ID cannot be empty.");
    }
}