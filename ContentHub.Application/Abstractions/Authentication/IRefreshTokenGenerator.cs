namespace ContentHub.Application.Abstractions.Authentication;

public interface IRefreshTokenGenerator
{
    string Generate();
    string Hash(string refreshToken);
}