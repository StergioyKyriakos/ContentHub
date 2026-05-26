namespace ContentHub.Api.Features.Auth.RefreshToken;


public sealed class RefreshTokenResponse
{
    public required string AccessToken { get; set; } 

    public required string RefreshToken { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}