using FluentValidation;

namespace ContentHub.Api.Features.AuditLogs.GetAuditLogById;

public sealed class GetAuditLogByIdQueryValidator : AbstractValidator<GetAuditLogByIdQuery>
{
    public GetAuditLogByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Audit Log ID cannot be empty.");
    }
}