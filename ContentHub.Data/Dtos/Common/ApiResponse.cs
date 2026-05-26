namespace ContentHub.Data.Dtos.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }

    public T? Data { get; set; }

    public ApiError? Error { get; set; }

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Error = null
        };
    }

    public static ApiResponse<T> Fail(ApiError error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = error
        };
    }
}