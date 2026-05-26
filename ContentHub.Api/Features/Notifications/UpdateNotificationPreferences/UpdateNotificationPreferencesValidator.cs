using FluentValidation;

namespace ContentHub.Api.Features.Notifications.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
{
    public UpdateNotificationPreferencesCommandValidator()
    {
        RuleFor(x => x.Preferences)
            .NotEmpty().WithMessage("Preferences list cannot be empty.");

        RuleForEach(x => x.Preferences).ChildRules(item =>
        {
            item.RuleFor(i => i.Type)
                .IsInEnum().WithMessage("Provided notification type value is invalid.");

            item.RuleFor(i => i.Channel)
                .IsInEnum().WithMessage("Provided notification channel value is invalid.");
        });
    }
}