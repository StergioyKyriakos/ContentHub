using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Assets;

public sealed class AssetFlowTests : IntegrationTestBase
{
    public AssetFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
    {
    }

    [Fact]
    public async Task UploadAsset_AttachToPost_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var assetId = await UploadTextFileAsPdfAsync();

        assetId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Asset_Get_Attach_Detach_Delete_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var assetId = await UploadTextFileAsPdfAsync();
        var post = await Cms.CreateDraftPostAsync();

        var getAssetsResponse = await GetAsJsonAsync("/api/assets", new
        {
        });

        getAssetsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAssetsBody = await LogResponseAsync(getAssetsResponse, "GET /api/assets response:");

        getAssetsBody.Should().Contain(assetId.ToString());

        var getByIdResponse = await GetAsJsonAsync($"/api/assets/{assetId}", new
        {
            id = assetId
        });

        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(getByIdResponse, $"GET /api/assets/{assetId} response:");

        var attachResponse = await Client.PostAsJsonAsync($"/api/posts/{post.Id}/assets/{assetId}", new
        {
            postId = post.Id,
            assetId
        });

        attachResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(attachResponse, $"POST /api/posts/{post.Id}/assets/{assetId} response:");

        var detachResponse = await DeleteAsJsonAsync($"/api/posts/{post.Id}/assets/{assetId}", new
        {
            postId = post.Id,
            assetId
        });

        detachResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(detachResponse, $"DELETE /api/posts/{post.Id}/assets/{assetId} response:");

        var deleteResponse = await DeleteAsJsonAsync($"/api/assets/{assetId}", new
        {
            id = assetId,
            force = true
        });

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(deleteResponse, $"DELETE /api/assets/{assetId} response:");
    }

    private async Task<Guid> UploadTextFileAsPdfAsync()
    {
        using var content = new MultipartFormDataContent();

        var fileBytes = "fake pdf content for integration test"u8.ToArray();
        var fileContent = new ByteArrayContent(fileBytes);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

        content.Add(fileContent, "file", "test.pdf");
        content.Add(new StringContent("Public"), "visibility");

        var response = await Client.PostAsync("/api/assets/upload", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(response, "POST /api/assets/upload response:");

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<UploadAssetData>>();

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        return body.Data!.Asset.Id;
    }

    private Task<HttpResponseMessage> DeleteAsJsonAsync(string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(body)
        };

        return Client.SendAsync(request);
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
