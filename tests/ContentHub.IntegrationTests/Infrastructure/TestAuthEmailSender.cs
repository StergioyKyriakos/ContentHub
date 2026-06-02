using System.Collections.Concurrent;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Entities.Users;

namespace ContentHub.IntegrationTests.Infrastructure;

public sealed class TestAuthEmailSender : IAuthEmailSender
{
    private readonly ConcurrentDictionary<string, string> _emailVerificationTokens = new();
    private readonly ConcurrentDictionary<string, string> _passwordResetTokens = new();

    public Task SendEmailVerificationAsync(
        User user,
        string token,
        CancellationToken cancellationToken = default)
    {
        _emailVerificationTokens[user.Email.ToUpperInvariant()] = token;

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        User user,
        string token,
        CancellationToken cancellationToken = default)
    {
        _passwordResetTokens[user.Email.ToUpperInvariant()] = token;

        return Task.CompletedTask;
    }

    public string GetEmailVerificationToken(string email)
    {
        return _emailVerificationTokens[email.ToUpperInvariant()];
    }

    public string GetPasswordResetToken(string email)
    {
        return _passwordResetTokens[email.ToUpperInvariant()];
    }
}
