using System.Net;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Systems;

public sealed class SystemFlowTests : IntegrationTestBase
{
    public SystemFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
    {
    }

    [Fact]
    public async Task Public_Can_Get_System_Info()
    {
        var response = await Client.GetAsync("/api/system/info");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await LogResponseAsync(response, "GET /api/system/info response:");

        body.Should().Contain("ContentHub");
    }
}
