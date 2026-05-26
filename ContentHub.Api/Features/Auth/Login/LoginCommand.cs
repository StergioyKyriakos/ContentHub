namespace ContentHub.Api.Features.Auth.Login;

public sealed class LoginCommand
{
    public required string EmailOrUsername { get; set; } 

    public required string Password { get; set; }
}