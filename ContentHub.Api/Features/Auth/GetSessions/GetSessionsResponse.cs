namespace ContentHub.Api.Features.Auth.GetSessions;

public sealed class GetSessionsResponse
{
    public IReadOnlyCollection<UserSessionResponse> Sessions { get; set; } = [];
}

public sealed class UserSessionResponse
{
    public Guid Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public bool IsCurrent { get; set; }
}
