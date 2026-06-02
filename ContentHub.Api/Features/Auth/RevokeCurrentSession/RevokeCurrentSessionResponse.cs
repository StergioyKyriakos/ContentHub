namespace ContentHub.Api.Features.Auth.RevokeCurrentSession;

public sealed class RevokeCurrentSessionResponse
{
    public Guid Id { get; set; }

    public bool Revoked { get; set; }
}
