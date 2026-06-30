using ContentHub.Application.Abstractions.Authentication;
using OtpNet;

namespace ContentHub.Infrastructure.Authentication;

public sealed class TotpTwoFactorService : ITwoFactorService
{
    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);

        return Base32Encoding.ToString(key);
    }

    public string BuildAuthenticatorUri(
        string issuer,
        string email,
        string secret)
    {
        return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
    }

    public bool VerifyCode(
        string secret,
        string code)
    {
        if (string.IsNullOrWhiteSpace(secret) ||
            string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var bytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(bytes);

        return totp.VerifyTotp(
            code.Trim(),
            out _,
            VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}