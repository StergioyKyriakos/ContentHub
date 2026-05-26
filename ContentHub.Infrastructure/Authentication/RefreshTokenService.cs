using ContentHub.Application.Abstractions.Authentication;

namespace ContentHub.Infrastructure.Authentication;

public sealed class RefreshTokenService
{
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RefreshTokenService(IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public string Generate()
    {
        return _refreshTokenGenerator.Generate();
    }

    public string Hash(string refreshToken)
    {
        return _refreshTokenGenerator.Hash(refreshToken);
    }
}