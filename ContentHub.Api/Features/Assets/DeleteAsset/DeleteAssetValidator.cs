using FluentValidation;

namespace ContentHub.Api.Features.Assets.DeleteAsset;

public sealed class DeleteAssetCommandValidator : AbstractValidator<DeleteAssetCommand>
{
    public DeleteAssetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Asset ID cannot be empty.");
    }
}