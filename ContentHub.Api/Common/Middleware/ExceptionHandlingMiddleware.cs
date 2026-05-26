using System.Net;
using System.Text.Json;
using ContentHub.Application.Common.Errors;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Entities.Common;
using FluentValidation;

namespace ContentHub.Api.Common.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException exception)
        {
            await HandleValidationExceptionAsync(context, exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            await HandleUnauthorizedExceptionAsync(context, exception);
        }
        catch (ForbiddenAccessException exception)
        {
            await HandleForbiddenExceptionAsync(context, exception);
        }
        catch (DomainException exception)
        {
            await HandleDomainExceptionAsync(context, exception);
        }
        catch (KeyNotFoundException exception)
        {
            await HandleNotFoundExceptionAsync(context, exception);
        }
        catch (Exception exception)
        {
            await HandleUnexpectedExceptionAsync(context, exception);
        }
    }

    private async Task HandleValidationExceptionAsync(
        HttpContext context,
        ValidationException exception)
    {
        var errors = exception.Errors
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

        await WriteResponseAsync(
            context,
            HttpStatusCode.BadRequest,
            ApiResponse<object>.Fail(apiError));
    }

    private async Task HandleUnauthorizedExceptionAsync(
        HttpContext context,
        UnauthorizedAccessException exception)
    {
        var apiError = ApiError.Create(
            code: "unauthorized",
            message: "Authentication is required.");

        await WriteResponseAsync(
            context,
            HttpStatusCode.Unauthorized,
            ApiResponse<object>.Fail(apiError));
    }

    private async Task HandleForbiddenExceptionAsync(
        HttpContext context,
        ForbiddenAccessException exception)
    {
        var apiError = ApiError.Create(
            code: "forbidden",
            message: "You do not have permission to access this resource.");

        await WriteResponseAsync(
            context,
            HttpStatusCode.Forbidden,
            ApiResponse<object>.Fail(apiError));
    }

    private async Task HandleDomainExceptionAsync(
        HttpContext context,
        DomainException exception)
    {
        var apiError = ApiError.Create(
            code: exception.Code,
            message: exception.Message);

        await WriteResponseAsync(
            context,
            exception.StatusCode,
            ApiResponse<object>.Fail(apiError));
    }

    private async Task HandleNotFoundExceptionAsync(
        HttpContext context,
        KeyNotFoundException exception)
    {
        var apiError = ApiError.Create(
            code: "not_found",
            message: exception.Message);

        await WriteResponseAsync(
            context,
            HttpStatusCode.NotFound,
            ApiResponse<object>.Fail(apiError));
    }

    private async Task HandleUnexpectedExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var correlationId = GetCorrelationId(context);

        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}",
            correlationId);

        var apiError = ApiError.Create(
            code: "internal_server_error",
            message: _environment.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred.");

        await WriteResponseAsync(
            context,
            HttpStatusCode.InternalServerError,
            ApiResponse<object>.Fail(apiError));
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        ApiResponse<object> response)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }

    private static string? GetCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var correlationId))
        {
            return correlationId?.ToString();
        }

        return null;
    }
}