using FluentValidation;

namespace ContentHub.Api.Features.Posts.ArchivePost;

public class ArchivePostValidator : AbstractValidator<ArchivePostCommand>
{
    public ArchivePostValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty");
    }
}