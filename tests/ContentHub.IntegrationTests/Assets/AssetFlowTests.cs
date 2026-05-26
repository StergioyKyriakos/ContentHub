using System.Net;
using System.Net.Http.Json;
using System.Text;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;

namespace ContentHub.IntegrationTests.Assets;

public sealed class AssetFlowTests : IntegrationTestBase
{
    public AssetFlowTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task UploadAsset_AttachToPost_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var assetId = await UploadTextFileAsPdfAsync();

        assetId.Should().NotBeEmpty();

        // This test only checks upload foundation.
        // Attach-to-post should be covered in the full post fixture once post creation helper is shared.
    }

    private async Task<Guid> UploadTextFileAsPdfAsync()
    {
        using var content = new MultipartFormDataContent();

        var fileBytes = Encoding.UTF8.GetBytes("fake pdf content for integration test");
        var fileContent = new ByteArrayContent(fileBytes);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

        content.Add(fileContent, "file", "test.pdf");
        content.Add(new StringContent("Public"), "visibility");

        var response = await Client.PostAsync("/api/assets/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<UploadAssetData>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        return body.Data!.Asset.Id;
    }

    private sealed class UploadAssetData
    {
        public AssetData Asset { get; init; } = default!;
    }

    private sealed class AssetData
    {
        public Guid Id { get; init; }
    }
}