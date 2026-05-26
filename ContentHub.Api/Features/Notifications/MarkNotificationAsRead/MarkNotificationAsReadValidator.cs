using FluentValidation;

namespace ContentHub.Api.Features.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Notification ID cannot be empty.");
    }
}