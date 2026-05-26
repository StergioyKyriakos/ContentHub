namespace ContentHub.Api.Features.Auth.Logout;

public class LogoutCommand
{
    public required string RefreshToken { get; set; }
}