using FluentValidation;
using Microsoft.Extensions.Options;
using ContentHub.Infrastructure.Storage;

namespace ContentHub.Api.Features.Assets.UploadAsset;

public sealed class UploadAssetCommandValidator : AbstractValidator<UploadAssetCommand>
{
    public UploadAssetCommandValidator(IOptions<StorageOptions> storageOptions)
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.")
            .Must(file => file.Length > 0).WithMessage("File cannot be empty.");

        RuleFor(x => x.File)
            .Must(file => file.Length <= storageOptions.Value.MaxFileSizeBytes)
            .WithMessage($"File size cannot exceed {storageOptions.Value.MaxFileSizeBytes / 1024 / 1024}MB.")
            .When(x => x.File is not null);

        RuleFor(x => x.File)
            .Must(file => storageOptions.Value.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            .WithMessage("The provided file content type is not supported.")
            .When(x => x.File is not null);

        RuleFor(x => x.Visibility)
            .IsInEnum().WithMessage("Provided asset visibility option is invalid.");
    }
}