namespace ContentHub.Application.Abstractions.Storage;

public interface IFileHashCalculator
{
    Task<string> CalculateHashAsync(
        Stream stream,
        CancellationToken cancellationToken = default);
}