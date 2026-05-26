using FluentValidation;

namespace ContentHub.Api.Features.Assets.AttachAssetToPost;

public sealed class AttachAssetToPostCommandValidator : AbstractValidator<AttachAssetToPostCommand>
{
    public AttachAssetToPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID cannot be empty.");

        RuleFor(x => x.AssetId)
            .NotEmpty().WithMessage("Asset ID cannot be empty.");
    }
}