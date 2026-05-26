using FluentValidation;

namespace ContentHub.Api.Features.Posts.SetFeaturedPost;

public class SetFeaturedPostValidator : AbstractValidator<SetFeaturedPostCommand>
{
    public SetFeaturedPostValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty");
    }
}