namespace ContentHub.IntegrationTests.Infrastructure;

public sealed class TestApiResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public TestApiError? Error { get; set; }
}

public sealed class TestApiError
{
    public string Code { get; set; } = null!;

    public string Message { get; set; } = null!;

    public Dictionary<string, string[]>? Details { get; set; }
}
