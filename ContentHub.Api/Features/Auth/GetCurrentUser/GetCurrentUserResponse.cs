namespace ContentHub.Api.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserResponse
{
    public Guid Id { get; set; }

    public required string Email { get; set; } 

    public required string Username { get; set; } 

    public required string DisplayName { get; set; } 

    public required string Status { get; set; }
}