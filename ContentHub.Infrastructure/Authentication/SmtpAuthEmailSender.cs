using System.Net;
using System.Net.Mail;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Entities.Users;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Authentication;

public sealed class SmtpAuthEmailSender : IAuthEmailSender
{
    private readonly SmtpEmailOptions _smtpOptions;
    private readonly AuthLinkOptions _authLinkOptions;

    public SmtpAuthEmailSender(
        IOptions<SmtpEmailOptions> smtpOptions,
        IOptions<AuthLinkOptions> authLinkOptions)
    {
        _smtpOptions = smtpOptions.Value;
        _authLinkOptions = authLinkOptions.Value;
    }

    public Task SendEmailVerificationAsync(
        User user,
        string token,
        CancellationToken cancellationToken = default)
    {
        var link = BuildLink(_authLinkOptions.VerifyEmailPath, user.Email, token);

        return SendAsync(
            toEmail: user.Email,
            subject: "Verify your ContentHub email address",
            body: $"Verify your email address by opening this link: {link}",
            cancellationToken);
    }

    public Task SendPasswordResetAsync(
        User user,
        string token,
        CancellationToken cancellationToken = default)
    {
        var link = BuildLink(_authLinkOptions.ResetPasswordPath, user.Email, token);

        return SendAsync(
            toEmail: user.Email,
            subject: "Reset your ContentHub password",
            body: $"Reset your password by opening this link: {link}",
            cancellationToken);
    }

    private async Task SendAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var message = new MailMessage
        {
            From = new MailAddress(_smtpOptions.FromEmail, _smtpOptions.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using var smtpClient = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
        {
            EnableSsl = _smtpOptions.UseSsl
        };

        if (!string.IsNullOrWhiteSpace(_smtpOptions.Username))
        {
            smtpClient.Credentials = new NetworkCredential(
                _smtpOptions.Username,
                _smtpOptions.Password);
        }

        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_smtpOptions.Host) ||
            string.IsNullOrWhiteSpace(_smtpOptions.FromEmail))
        {
            throw new InvalidOperationException("SMTP email delivery is not configured.");
        }
    }

    private string BuildLink(string path, string email, string token)
    {
        var baseUrl = _authLinkOptions.PublicBaseUrl.TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);

        return $"{baseUrl}{normalizedPath}?email={encodedEmail}&token={encodedToken}";
    }
}
