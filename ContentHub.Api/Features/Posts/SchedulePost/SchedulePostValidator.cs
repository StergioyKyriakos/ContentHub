using FluentValidation;

namespace ContentHub.Api.Features.Posts.SchedulePost;

public sealed class SchedulePostValidator : AbstractValidator<SchedulePostCommand>
{
    public SchedulePostValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty");
        
        RuleFor(command => command.ScheduledForUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Scheduled date must be in the future.");
    }
}