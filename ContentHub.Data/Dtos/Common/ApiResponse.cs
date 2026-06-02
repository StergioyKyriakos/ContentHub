namespace ContentHub.Data.Dtos.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public ApiError? Error { get; set; }

    public static ApiResponse<T> Ok(
        T data,
        string message = "Request completed successfully.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Error = null
        };
    }

    public static ApiResponse<T> Fail(ApiError error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = error.Message,
            Data = default,
            Error = error
        };
    }
}
