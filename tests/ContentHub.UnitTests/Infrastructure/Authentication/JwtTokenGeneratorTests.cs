using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ContentHub.Data.Entities.Users;
using ContentHub.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ContentHub.UnitTests.Infrastructure.Authentication;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void Generate_Should_Return_Valid_Jwt()
    {
        var generator = CreateGenerator();

        var user = CreateUser();

        var token = generator.Generate(user, ["Admin"]);

        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();

        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void Generate_Should_Include_User_Claims()
    {
        var generator = CreateGenerator();

        var user = CreateUser();

        var token = generator.Generate(user, ["Admin"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(claim =>
            claim.Type == JwtRegisteredClaimNames.Email &&
            claim.Value == user.Email);

        jwt.Claims.Should().Contain(claim =>
            claim.Type == "userId" &&
            claim.Value == user.Id.ToString());
    }

    [Fact]
    public void Generate_Should_Include_Role_Claims()
    {
        var generator = CreateGenerator();

        var user = CreateUser();

        var token = generator.Generate(user, ["Admin", "Editor"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(claim =>
            claim.Type == ClaimTypes.Role &&
            claim.Value == "Admin");

        jwt.Claims.Should().Contain(claim =>
            claim.Type == ClaimTypes.Role &&
            claim.Value == "Editor");
    }

    private static JwtTokenGenerator CreateGenerator()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "ContentHub.Tests",
            Audience = "ContentHub.Tests",
            Secret = "THIS_IS_A_TEST_SECRET_KEY_FOR_UNIT_TESTS_123456789",
            ExpirationMinutes = 15,
            RefreshTokenExpirationDays = 30
        });

        return new JwtTokenGenerator(options);
    }

    private static User CreateUser()
    {
        return new User(
            email: "test@contenthub.local",
            username: "test_user",
            displayName: "test_user",
            passwordHash: "hashed-password");
    }
}