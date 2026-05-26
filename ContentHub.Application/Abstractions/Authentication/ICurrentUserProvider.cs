namespace ContentHub.Application.Abstractions.Authentication;

public interface ICurrentUserProvider
{
     Guid? UserId { get; }
     string? Email { get; }
     bool IsAuthenticated { get; }
}