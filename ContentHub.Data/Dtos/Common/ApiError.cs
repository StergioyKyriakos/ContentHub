namespace ContentHub.Data.Dtos.Common;

public sealed class ApiError
{
    public string Code { get; set; } = null!;

    public string Message { get; set; } = null!;

    public Dictionary<string, string[]>? Details { get; set; }

    public static ApiError Create(
        string code,
        string message,
        Dictionary<string, string[]>? details = null)
    {
        return new ApiError
        {
            Code = code,
            Message = message,
            Details = details
        };
    }
}