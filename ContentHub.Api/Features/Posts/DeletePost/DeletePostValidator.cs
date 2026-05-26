using FluentValidation;

namespace ContentHub.Api.Features.Posts.DeletePost;

public class DeletePostValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostValidator()
    {
        RuleFor(x => x.id).NotEmpty().WithMessage("Id cannot be empty");
    }
}