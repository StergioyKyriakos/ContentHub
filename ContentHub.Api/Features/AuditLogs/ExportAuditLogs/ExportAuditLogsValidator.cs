using FluentValidation;

namespace ContentHub.Api.Features.AuditLogs.ExportAuditLogs;

public sealed class ExportAuditLogsValidator : AbstractValidator<ExportAuditLogsCommand>
{
    public ExportAuditLogsValidator()
    {
        RuleFor(command => command.ActorUserId)
            .NotEmpty()
            .When(command => command.ActorUserId.HasValue)
            .WithMessage("Actor User ID cannot be an empty GUID.");

        RuleFor(command => command.Action)
            .IsInEnum()
            .When(command => command.Action.HasValue)
            .WithMessage("Provided audit action filter is invalid.");

        RuleFor(command => command.EntityName)
            .MaximumLength(200)
            .WithMessage("Entity name filter cannot exceed 200 characters.");

        RuleFor(command => command.EntityId)
            .MaximumLength(100)
            .WithMessage("Entity ID filter cannot exceed 100 characters.");

        RuleFor(command => command.From)
            .LessThanOrEqualTo(command => command.To!.Value)
            .When(command => command.From.HasValue && command.To.HasValue)
            .WithMessage("The 'From' date boundary cannot occur after the 'To' date boundary.");
    }
}
