namespace ContentHub.Infrastructure.Authentication;

public sealed class SmtpEmailOptions
{
    public const string SectionName = "Email:Smtp";

    public bool Enabled { get; set; }

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 1025;

    public bool UseSsl { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string FromEmail { get; set; } = "no-reply@contenthub.local";

    public string FromName { get; set; } = "ContentHub";
}
