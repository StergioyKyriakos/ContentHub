using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Entities.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Authentication;

public sealed class DevelopmentAuthEmailSender : IAuthEmailSender
{
    private readonly ILogger<DevelopmentAuthEmailSender> _logger;
    private readonly AuthLinkOptions _options;

    public DevelopmentAuthEmailSender(
        ILogger<DevelopmentAuthEmailSender> logger,
        IOptions<AuthLinkOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendEmailVerificationAsync(
        User user,
        string token,
        CancellationToken cancellationToken = default)
    {
        var link = BuildLink(_options.VerifyEmailPath, user.Email, token);

        _logger.LogInformation(
            "Email verification link for {Email}: {Link}",
            user.Email,
            link);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        User user,
        string token,
        CancellationToken cancellationToken = default)
    {
        var link = BuildLink(_options.ResetPasswordPath, user.Email, token);

        _logger.LogInformation(
            "Password reset link for {Email}: {Link}",
            user.Email,
            link);

        return Task.CompletedTask;
    }

    private string BuildLink(string path, string email, string token)
    {
        var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);

        return $"{baseUrl}{normalizedPath}?email={encodedEmail}&token={encodedToken}";
    }
}
