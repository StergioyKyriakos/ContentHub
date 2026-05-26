namespace ContentHub.Data.Dtos.Auth;

public sealed class AuthUserDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; } 
    public required string Status { get; set; } 
    public bool EmailVerified { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = [];
}