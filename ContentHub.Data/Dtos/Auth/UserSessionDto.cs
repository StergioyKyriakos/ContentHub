namespace ContentHub.Data.Dtos.Auth;

public sealed class UserSessionDto
{
    public Guid Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public bool IsActive { get; set; }
}