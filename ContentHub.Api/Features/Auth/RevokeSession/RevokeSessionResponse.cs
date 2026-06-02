namespace ContentHub.Api.Features.Auth.RevokeSession;

public sealed class RevokeSessionResponse
{
    public Guid Id { get; set; }

    public bool Revoked { get; set; }
}
