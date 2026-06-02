using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Entities.Users;
using ContentHub.Data.Persistence;
using ContentHub.Data.Persistence.Seed;
using ContentHub.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Auth;

public sealed class AuthFlowTests : IntegrationTestBase
{
    public AuthFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
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

        await LogResponseAsync(registerResponse, "POST /api/auth/register response:");

        var token = await Auth.LoginAsync(email);

        Auth.UseBearerToken(token);

        var currentUserResponse = await Client.GetAsync("/api/auth/me");

        currentUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await LogResponseAsync(currentUserResponse, "GET /api/auth/me response:");

        body.Should().Contain(email);
    }

    [Fact]
    public async Task Register_VerifyEmail_Should_Work()
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

        await LogResponseAsync(registerResponse, "POST /api/auth/register response:");

        var emailSender = Factory.Services.GetRequiredService<TestAuthEmailSender>();
        var token = emailSender.GetEmailVerificationToken(email);

        var verifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-email", new
        {
            email,
            token
        });

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(verifyResponse, "POST /api/auth/verify-email response:");

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        var user = await db.Users.FirstAsync(user => user.NormalizedEmail == email.ToUpperInvariant());

        user.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task RequestEmailVerification_Should_Send_New_Token()
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

        await LogResponseAsync(registerResponse, "POST /api/auth/register response:");

        var requestResponse = await Client.PostAsJsonAsync("/api/auth/email-verification/request", new
        {
            email
        });

        requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(requestResponse, "POST /api/auth/email-verification/request response:");

        var emailSender = Factory.Services.GetRequiredService<TestAuthEmailSender>();

        emailSender.GetEmailVerificationToken(email).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task VerifyEmail_With_Reused_Token_Should_Fail()
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

        await LogResponseAsync(registerResponse, "POST /api/auth/register response:");

        var emailSender = Factory.Services.GetRequiredService<TestAuthEmailSender>();
        var token = emailSender.GetEmailVerificationToken(email);

        var firstVerifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-email", new
        {
            email,
            token
        });

        firstVerifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(firstVerifyResponse, "POST /api/auth/verify-email first response:");

        var secondVerifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-email", new
        {
            email,
            token
        });

        secondVerifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await LogResponseAsync(secondVerifyResponse, "POST /api/auth/verify-email reused response:");
    }

    [Fact]
    public async Task VerifyEmail_With_Expired_Token_Should_Fail()
    {
        var faker = new Faker();

        var email = faker.Internet.Email().ToLowerInvariant();
        var username = faker.Internet.UserName().Replace(".", "_").ToLowerInvariant();
        var token = "expired-verification-token";

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var securityTokenGenerator = scope.ServiceProvider.GetRequiredService<ISecurityTokenGenerator>();

        var user = new User(
            email: email,
            username: username,
            displayName: username,
            passwordHash: passwordHasher.Hash(TestConstants.DefaultPassword));

        user.AddEmailVerificationToken(
            tokenHash: securityTokenGenerator.Hash(token),
            expiresAtUtc: DateTime.UtcNow.AddMinutes(-5),
            userAgent: null,
            ipAddress: null);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var verifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-email", new
        {
            email,
            token
        });

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await LogResponseAsync(verifyResponse, "POST /api/auth/verify-email expired response:");
    }

    [Fact]
    public async Task ForgotPassword_ResetPassword_Should_Work()
    {
        var faker = new Faker();

        var email = faker.Internet.Email().ToLowerInvariant();
        var username = faker.Internet.UserName().Replace(".", "_").ToLowerInvariant();
        var newPassword = "NewPassword123!";

        await Seeder.CreateUserAsync(email, username, "Viewer");

        var forgotResponse = await Client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email
        });

        forgotResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(forgotResponse, "POST /api/auth/forgot-password response:");

        var emailSender = Factory.Services.GetRequiredService<TestAuthEmailSender>();
        var token = emailSender.GetPasswordResetToken(email);

        var resetResponse = await Client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            email,
            token,
            newPassword
        });

        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(resetResponse, "POST /api/auth/reset-password response:");

        var oldLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUsername = email,
            password = TestConstants.DefaultPassword
        });

        oldLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await LogResponseAsync(oldLoginResponse, "POST /api/auth/login old password response:");

        var newLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUsername = email,
            password = newPassword
        });

        newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(newLoginResponse, "POST /api/auth/login new password response:");
    }

    [Fact]
    public async Task ResetPassword_With_Invalid_Token_Should_Fail()
    {
        var faker = new Faker();

        var email = faker.Internet.Email().ToLowerInvariant();
        var username = faker.Internet.UserName().Replace(".", "_").ToLowerInvariant();

        await Seeder.CreateUserAsync(email, username, "Viewer");

        var resetResponse = await Client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            email,
            token = "invalid-reset-token",
            newPassword = "NewPassword123!"
        });

        resetResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await LogResponseAsync(resetResponse, "POST /api/auth/reset-password invalid token response:");
    }

    [Fact]
    public async Task ResetPassword_Should_Revoke_Existing_RefreshTokens()
    {
        var faker = new Faker();

        var email = faker.Internet.Email().ToLowerInvariant();
        var username = faker.Internet.UserName().Replace(".", "_").ToLowerInvariant();

        await Seeder.CreateUserAsync(email, username, "Viewer");

        var loginData = await Auth.LoginWithRefreshAsync(email);

        var forgotResponse = await Client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email
        });

        forgotResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(forgotResponse, "POST /api/auth/forgot-password response:");

        var emailSender = Factory.Services.GetRequiredService<TestAuthEmailSender>();
        var token = emailSender.GetPasswordResetToken(email);

        var resetResponse = await Client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            email,
            token,
            newPassword = "NewPassword123!"
        });

        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(resetResponse, "POST /api/auth/reset-password response:");

        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            refreshToken = loginData.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await LogResponseAsync(refreshResponse, "POST /api/auth/refresh-token revoked response:");
    }

    [Fact]
    public async Task RefreshToken_Should_Rotate_Token()
    {
        var loginData = await Auth.LoginWithRefreshAsync(TestConstants.AdminEmail);

        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            refreshToken = loginData.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await LogResponseAsync(refreshResponse, "POST /api/auth/refresh-token response:");

        body.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Logout_Should_Revoke_RefreshToken()
    {
        var loginData = await Auth.LoginWithRefreshAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(loginData.AccessToken);

        var logoutResponse = await Client.PostAsJsonAsync("/api/auth/logout", new
        {
            refreshToken = loginData.RefreshToken
        });

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(logoutResponse, "POST /api/auth/logout response:");

        Auth.ClearBearerToken();

        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            refreshToken = loginData.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await LogResponseAsync(refreshResponse, "POST /api/auth/refresh-token after logout response:");
    }

    [Fact]
    public async Task Sessions_Should_List_And_Revoke_Current_Session()
    {
        var loginData = await Auth.LoginWithRefreshAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(loginData.AccessToken);

        var sessionsResponse = await GetAsJsonAsync("/api/auth/sessions", new
        {
        });

        sessionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(sessionsResponse, "GET /api/auth/sessions response:");

        var sessionsBody = await sessionsResponse.Content.ReadFromJsonAsync<TestApiResponse<GetSessionsData>>();

        sessionsBody.Should().NotBeNull();
        sessionsBody!.Data.Should().NotBeNull();
        sessionsBody.Data!.Sessions.Should().Contain(session => session.IsCurrent);

        var revokeResponse = await DeleteAsJsonAsync("/api/auth/sessions/current", new
        {
        });

        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(revokeResponse, "DELETE /api/auth/sessions/current response:");

        Auth.ClearBearerToken();

        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            refreshToken = loginData.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await LogResponseAsync(refreshResponse, "POST /api/auth/refresh-token revoked current session response:");
    }

    [Fact]
    public async Task Sessions_Should_Revoke_Specific_Session()
    {
        var firstLogin = await Auth.LoginWithRefreshAsync(TestConstants.AdminEmail);
        var secondLogin = await Auth.LoginWithRefreshAsync(TestConstants.AdminEmail);

        Auth.UseBearerToken(firstLogin.AccessToken);

        var sessionsResponse = await GetAsJsonAsync("/api/auth/sessions", new
        {
        });

        sessionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(sessionsResponse, "GET /api/auth/sessions response:");

        var sessionsBody = await sessionsResponse.Content.ReadFromJsonAsync<TestApiResponse<GetSessionsData>>();
        var sessionToRevoke = sessionsBody!.Data!.Sessions.Single(session => !session.IsCurrent);

        var revokeResponse = await DeleteAsJsonAsync($"/api/auth/sessions/{sessionToRevoke.Id}", new
        {
            id = sessionToRevoke.Id
        });

        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(revokeResponse, $"DELETE /api/auth/sessions/{sessionToRevoke.Id} response:");

        Auth.ClearBearerToken();

        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            refreshToken = secondLogin.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await LogResponseAsync(refreshResponse, "POST /api/auth/refresh-token revoked specific session response:");
    }

    [Fact]
    public async Task Sessions_Should_Revoke_All_Sessions()
    {
        var firstLogin = await Auth.LoginWithRefreshAsync(TestConstants.AdminEmail);
        var secondLogin = await Auth.LoginWithRefreshAsync(TestConstants.AdminEmail);

        Auth.UseBearerToken(firstLogin.AccessToken);

        var revokeAllResponse = await DeleteAsJsonAsync("/api/auth/sessions", new
        {
        });

        revokeAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(revokeAllResponse, "DELETE /api/auth/sessions response:");

        Auth.ClearBearerToken();

        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            refreshToken = secondLogin.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await LogResponseAsync(refreshResponse, "POST /api/auth/refresh-token after revoke all response:");
    }

    [Fact]
    public async Task Sessions_Require_Authentication()
    {
        Auth.ClearBearerToken();

        var response = await GetAsJsonAsync("/api/auth/sessions", new
        {
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await LogResponseAsync(response, "GET /api/auth/sessions unauthorized response:");
    }

    [Fact]
    public async Task Viewer_Should_Be_Forbidden_From_Admin_AuditLogs()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var response = await GetAsJsonAsync("/api/admin/audit-logs", new
        {
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await LogResponseAsync(response, "GET /api/admin/audit-logs forbidden response:");
    }

    [Fact]
    public async Task DatabaseSeeder_Should_Create_Sample_Content()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

        await seeder.SeedAsync(db);

        var categoriesCount = await db.Categories.CountAsync();
        var authorsCount = await db.Authors.CountAsync();
        var postsCount = await db.Posts.CountAsync();

        categoriesCount.Should().BeGreaterThanOrEqualTo(3);
        authorsCount.Should().BeGreaterThanOrEqualTo(1);
        postsCount.Should().BeGreaterThanOrEqualTo(3);
    }

    private sealed class GetSessionsData
    {
        public List<SessionData> Sessions { get; init; } = [];
    }

    private Task<HttpResponseMessage> DeleteAsJsonAsync(string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(body)
        };

        return Client.SendAsync(request);
    }

    private sealed class SessionData
    {
        public Guid Id { get; init; }

        public bool IsCurrent { get; init; }
    }
}
