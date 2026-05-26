using ContentHub.Infrastructure.Authentication;
using FluentAssertions;

namespace ContentHub.UnitTests.Infrastructure.Authentication;

public sealed class RefreshTokenGeneratorTests
{
    private readonly RefreshTokenGenerator _generator = new();

    [Fact]
    public void Generate_Should_Return_NonEmpty_Token()
    {
        var token = _generator.Generate();

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generate_Should_Return_Different_Tokens()
    {
        var token1 = _generator.Generate();
        var token2 = _generator.Generate();

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void Hash_Should_Return_Same_Hash_For_Same_Token()
    {
        var token = _generator.Generate();

        var hash1 = _generator.Hash(token);
        var hash2 = _generator.Hash(token);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Hash_Should_Not_Return_Original_Token()
    {
        var token = _generator.Generate();

        var hash = _generator.Hash(token);

        hash.Should().NotBe(token);
    }
}