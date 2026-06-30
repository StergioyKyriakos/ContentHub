namespace ContentHub.Application.Abstractions.Authentication;

public interface ITwoFactorService
{
    string GenerateSecret();

    string BuildAuthenticatorUri(
        string issuer,
        string email,
        string secret);

    bool VerifyCode(
        string secret,
        string code);
}