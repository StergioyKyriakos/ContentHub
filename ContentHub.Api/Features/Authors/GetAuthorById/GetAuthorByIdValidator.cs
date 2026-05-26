using FluentValidation;

namespace ContentHub.Api.Features.Authors.GetAuthorById;

public class GetAuthorByIdValidator : AbstractValidator<GetAuthorByIdQuery>
{
    public GetAuthorByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
    }
}