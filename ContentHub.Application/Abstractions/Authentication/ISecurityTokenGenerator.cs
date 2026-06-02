namespace ContentHub.Application.Abstractions.Authentication;

public interface ISecurityTokenGenerator
{
    string Generate();
    string Hash(string token);
}
