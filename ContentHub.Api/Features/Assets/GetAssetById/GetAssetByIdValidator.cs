using FluentValidation;

namespace ContentHub.Api.Features.Assets.GetAssetById;

public sealed class GetAssetByIdQueryValidator : AbstractValidator<GetAssetByIdQuery>
{
    public GetAssetByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Asset ID cannot be empty.");
    }
}