using FluentValidation;

namespace ContentHub.Api.Features.Posts.RemoveFeaturedPost;

public class RemoveFeaturedPostValidator : AbstractValidator<RemoveFeaturedPostCommand>
{
    public RemoveFeaturedPostValidator()
    {
        RuleFor(x => x.Id).NotNull().WithMessage("Id cannot be null");
    }
}