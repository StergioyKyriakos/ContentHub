using FluentValidation;

namespace ContentHub.Api.Features.Posts.CreatePost;

public sealed class CreatePostValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage("Post title is required.")
            .MaximumLength(250)
            .WithMessage("Post title must be 250 characters or fewer.");

        RuleFor(command => command.Slug)
            .MaximumLength(280)
            .WithMessage("Post slug must be 280 characters or fewer.")
            .When(command => !string.IsNullOrWhiteSpace(command.Slug));

        RuleFor(command => command.Summary)
            .MaximumLength(1000)
            .WithMessage("Post summary must be 1000 characters or fewer.")
            .When(command => !string.IsNullOrWhiteSpace(command.Summary));

        RuleFor(command => command.Content)
            .NotEmpty()
            .WithMessage("Post content is required.");

        RuleForEach(command => command.Tags)
            .MaximumLength(100)
            .WithMessage("Each tag must be 100 characters or fewer.");
    }
}
