using ContentHub.Data.Dtos.Common;

namespace ContentHub.Api.Common.ApiResults;

public static class ResultsFactory
{
    public static IResult Ok<T>(T data)
    {
        return Results.Ok(ApiResponse<T>.Ok(data));
    }

    public static IResult Created<T>(string uri, T data)
    {
        return Results.Created(uri, ApiResponse<T>.Ok(data));
    }

    public static IResult BadRequest(string code, string message)
    {
        var error = ApiError.Create(code, message);

        return Results.BadRequest(ApiResponse<object>.Fail(error));
    }

    public static IResult ValidationProblem(IDictionary<string, string[]> errors)
    {
        var details = errors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value);

        var error = ApiError.Create(
            code: "validation_failed",
            message: "One or more validation errors occurred.",
            details: details);

        return Results.BadRequest(ApiResponse<object>.Fail(error));
    }

    public static IResult NotFound(string code, string message)
    {
        var error = ApiError.Create(code, message);

        return Results.NotFound(ApiResponse<object>.Fail(error));
    }

    public static IResult Unauthorized(string message = "Authentication is required.")
    {
        var error = ApiError.Create(
            code: "unauthorized",
            message: message);

        return Results.Json(
            ApiResponse<object>.Fail(error),
            statusCode: StatusCodes.Status401Unauthorized);
    }

    public static IResult Forbidden(string message = "You do not have permission to access this resource.")
    {
        var error = ApiError.Create(
            code: "forbidden",
            message: message);

        return Results.Json(
            ApiResponse<object>.Fail(error),
            statusCode: StatusCodes.Status403Forbidden);
    }

    public static IResult Conflict(string code, string message)
    {
        var error = ApiError.Create(code, message);

        return Results.Conflict(ApiResponse<object>.Fail(error));
    }
}
