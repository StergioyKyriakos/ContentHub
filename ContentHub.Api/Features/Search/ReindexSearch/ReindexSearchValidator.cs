using FluentValidation;

namespace ContentHub.Api.Features.Search.ReindexSearch;

public sealed class ReindexSearchCommandValidator : AbstractValidator<ReindexSearchCommand>
{
    public ReindexSearchCommandValidator()
    {
        RuleFor(x => x.Force)
            .NotNull().WithMessage("Force value is required.");
    }
}
