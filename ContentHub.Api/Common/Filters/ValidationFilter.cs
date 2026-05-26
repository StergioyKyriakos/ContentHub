using ContentHub.Data.Dtos.Common;
using FluentValidation;

namespace ContentHub.Api.Common.Filters;

public sealed class ValidationFilter<TRequest> : IEndpointFilter
{
    private readonly IValidator<TRequest> _validator;

    public ValidationFilter(IValidator<TRequest> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments
            .OfType<TRequest>()
            .FirstOrDefault();

        if (request is null)
        {
            return await next(context);
        }

        var validationResult = await _validator.ValidateAsync(
            request,
            context.HttpContext.RequestAborted);

        if (validationResult.IsValid)
        {
            return await next(context);
        }

        var errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        var apiError = ApiError.Create(
            code: "validation_failed",
            message: "One or more validation errors occurred.",
            details: errors);

        return Results.BadRequest(ApiResponse<object>.Fail(apiError));
    }
}