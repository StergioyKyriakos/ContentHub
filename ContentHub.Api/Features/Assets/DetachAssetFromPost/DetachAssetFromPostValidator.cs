using FluentValidation;

namespace ContentHub.Api.Features.Assets.DetachAssetFromPost;

public sealed class DetachAssetFromPostCommandValidator : AbstractValidator<DetachAssetFromPostCommand>
{
    public DetachAssetFromPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID cannot be empty.");

        RuleFor(x => x.AssetId)
            .NotEmpty().WithMessage("Asset ID cannot be empty.");
    }
}