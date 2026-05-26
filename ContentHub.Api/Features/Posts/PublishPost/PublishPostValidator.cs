using FluentValidation;

namespace ContentHub.Api.Features.Posts.PublishPost;

public class PublishPostValidator : AbstractValidator<PublishPostCommand>
{
    public PublishPostValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty");
    }
}