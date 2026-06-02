using ContentHub.Data.Entities.Users;

namespace ContentHub.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    string Generate(
        User user,
        IReadOnlyCollection<string> roles,
        Guid? sessionId = null);
}
