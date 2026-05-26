using FluentValidation;

namespace ContentHub.Api.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100 entries.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Provided notification status filter value is invalid.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Provided notification type filter value is invalid.")
            .When(x => x.Type.HasValue);
    }
}