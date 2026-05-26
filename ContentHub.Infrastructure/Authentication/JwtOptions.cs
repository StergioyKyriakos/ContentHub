namespace ContentHub.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = null!;

    public string Audience { get; set; } = null!;

    public string Secret { get; set; } = null!;

    public int ExpirationMinutes { get; set; } = 60;

    public int RefreshTokenExpirationDays { get; set; } = 30;
}