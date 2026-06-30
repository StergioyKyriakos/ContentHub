namespace ContentHub.Api.Features.Auth.Login;

public sealed class LoginResponse
{
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; } 

    public DateTime? ExpiresAtUtc { get; set; }

    public AuthUserResponse? User { get; set; } 
    
    public bool RequiresTwoFactor { get; set; }

    public Guid? UserId { get; set; }
}

public sealed class AuthUserResponse
{
    public Guid Id { get; set; }

    public required string Email { get; set; } 

    public required string Username { get; set; } 

    public required string DisplayName { get; set; } 
}
