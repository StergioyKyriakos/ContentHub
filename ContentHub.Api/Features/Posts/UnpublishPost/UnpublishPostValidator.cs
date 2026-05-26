using FluentValidation;

namespace ContentHub.Api.Features.Posts.UnpublishPost;

public class UnpublishPostValidator : AbstractValidator<UnpublishPostCommand>
{
    public UnpublishPostValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty");
    }
}