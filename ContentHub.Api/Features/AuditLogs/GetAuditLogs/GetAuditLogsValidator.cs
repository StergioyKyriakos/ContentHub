using FluentValidation;

namespace ContentHub.Api.Features.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100 entries.");

        RuleFor(x => x.ActorUserId)
            .NotEmpty()
            .When(x => x.ActorUserId.HasValue)
            .WithMessage("Actor User ID cannot be an empty GUID.");

        RuleFor(x => x.Action)
            .IsInEnum()
            .When(x => x.Action.HasValue)
            .WithMessage("Provided audit action filter is invalid.");

        RuleFor(x => x.EntityName)
            .MaximumLength(200)
            .WithMessage("Entity name filter cannot exceed 200 characters.");

        RuleFor(x => x.EntityId)
            .MaximumLength(100)
            .WithMessage("Entity ID filter cannot exceed 100 characters.");

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("The 'From' date boundary cannot occur after the 'To' date boundary.");
    }
}