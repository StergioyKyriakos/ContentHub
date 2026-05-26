using FluentValidation;

namespace ContentHub.Api.Features.Authors.DeleteAuthor;

public sealed class DeleteAuthorValidator : AbstractValidator<DeleteAuthorCommand>
{
    public DeleteAuthorValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}