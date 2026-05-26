namespace ContentHub.Api.Features.System.GetSystemInfo;

public sealed class GetSystemInfoResponse
{
    public string ApplicationName { get; set; } = null!;

    public string Version { get; set; } = null!;

    public string Environment { get; set; } = null!;

    public string MachineName { get; set; } = null!;

    public DateTime ServerTimeUtc { get; set; }
}