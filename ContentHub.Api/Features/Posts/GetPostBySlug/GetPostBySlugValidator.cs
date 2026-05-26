using FluentValidation;

namespace ContentHub.Api.Features.Posts.GetPostBySlug;

public class GetPostBySlugValidator : AbstractValidator<GetPostBySlugQuery>
{
    public GetPostBySlugValidator()
    {
        RuleFor(command => command.Slug).NotEmpty().WithMessage("Slug cannot be empty");
    }
}