namespace ContentHub.Api.Features.Auth.Register;

public sealed class RegisterResponse
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public required string Username { get; set; } 

    public required string DisplayName { get; set; } 
}