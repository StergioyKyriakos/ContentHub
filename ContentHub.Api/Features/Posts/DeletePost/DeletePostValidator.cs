using FluentValidation;

namespace ContentHub.Api.Features.Posts.DeletePost;

public sealed class DeletePostValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty");
    }
}
