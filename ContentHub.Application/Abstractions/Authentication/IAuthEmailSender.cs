using ContentHub.Data.Entities.Users;

namespace ContentHub.Application.Abstractions.Authentication;

public interface IAuthEmailSender
{
    Task SendEmailVerificationAsync(
        User user,
        string token,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        User user,
        string token,
        CancellationToken cancellationToken = default);
}
