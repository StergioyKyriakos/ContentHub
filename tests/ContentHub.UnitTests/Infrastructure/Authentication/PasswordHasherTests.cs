using ContentHub.Infrastructure.Authentication;
using FluentAssertions;

namespace ContentHub.UnitTests.Infrastructure.Authentication;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public void Hash_Should_Return_NonEmpty_Hash()
    {
        var hash = _passwordHasher.Hash("Password123!");

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe("Password123!");
    }

    [Fact]
    public void Verify_Should_Return_True_For_Correct_Password()
    {
        var hash = _passwordHasher.Hash("Password123!");

        var result = _passwordHasher.Verify("Password123!", hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_Should_Return_False_For_Wrong_Password()
    {
        var hash = _passwordHasher.Hash("Password123!");

        var result = _passwordHasher.Verify("WrongPassword123!", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_Should_Generate_Different_Hashes_For_Same_Password()
    {
        var hash1 = _passwordHasher.Hash("Password123!");
        var hash2 = _passwordHasher.Hash("Password123!");

        hash1.Should().NotBe(hash2);
    }
}