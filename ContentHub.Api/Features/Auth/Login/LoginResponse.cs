namespace ContentHub.Api.Features.Auth.Login;

public sealed class LoginResponse
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; } 

    public DateTime ExpiresAtUtc { get; set; }

    public required AuthUserResponse User { get; set; } 
}

public sealed class AuthUserResponse
{
    public Guid Id { get; set; }

    public required string Email { get; set; } 

    public required string Username { get; set; } 

    public required string DisplayName { get; set; } 
}