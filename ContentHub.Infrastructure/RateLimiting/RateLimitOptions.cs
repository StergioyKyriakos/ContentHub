namespace ContentHub.Infrastructure.RateLimiting;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public bool FailClosedWhenUnavailable { get; set; }

    public LoginRateLimitOptions Login { get; set; } = new();
}

public sealed class LoginRateLimitOptions
{
    public int MaxFailedAttempts { get; set; } = 5;

    public int WindowMinutes { get; set; } = 10;
}
