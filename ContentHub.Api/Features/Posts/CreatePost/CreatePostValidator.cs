using FluentValidation;

namespace ContentHub.Api.Features.Posts.CreatePost;

public sealed class CreatePostValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(command => command.Slug)
            .MaximumLength(280)
            .When(command => !string.IsNullOrWhiteSpace(command.Slug));

        RuleFor(command => command.Summary)
            .MaximumLength(1000)
            .When(command => !string.IsNullOrWhiteSpace(command.Summary));

        RuleFor(command => command.Content)
            .NotEmpty();

        RuleForEach(command => command.Tags)
            .MaximumLength(100);
    }
}