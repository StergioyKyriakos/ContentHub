using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;

namespace ContentHub.IntegrationTests.Auth;

public sealed class AuthFlowTests : IntegrationTestBase
{
    public AuthFlowTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Register_Login_GetCurrentUser_Should_Work()
    {
        var faker = new Faker();

        var email = faker.Internet.Email().ToLowerInvariant();
        var username = faker.Internet.UserName().Replace(".", "_").ToLowerInvariant();

        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            username,
            displayName = username,
            password = TestConstants.DefaultPassword
        });

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await Auth.LoginAsync(email);

        Auth.UseBearerToken(token);

        var currentUserResponse = await Client.GetAsync("/api/auth/me");

        currentUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await currentUserResponse.Content.ReadAsStringAsync();

        body.Should().Contain(email);
    }
}
