namespace ContentHub.Api.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommand
{
    public required string RefreshToken { get; set; }
}