using System.Security.Claims;
using System.Text.Json;
using System.Text;
using ContentHub.Data.Enums;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ContentHub.Api.Extensions;

public static class AuthenticationExtensions
{
    public const string ExternalAuthenticationScheme = "External";
    public const string GoogleAuthenticationScheme = "Google";
    public const string GitHubAuthenticationScheme = "GitHub";

    public static IServiceCollection AddContentHubAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>();

        if (jwtOptions is null)
        {
            throw new InvalidOperationException("JWT configuration is missing.");
        }

        var authenticationBuilder = services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = ExternalAuthenticationScheme;
            })
            .AddCookie(ExternalAuthenticationScheme, options =>
            {
                options.Cookie.Name = "contenthub.external";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
            });

        if (OAuthProviderIsConfigured(configuration, "Google"))
        {
            authenticationBuilder.AddOAuth(GoogleAuthenticationScheme, options =>
            {
                options.ClientId = configuration["OAuth:Google:ClientId"]!;
                options.ClientSecret = configuration["OAuth:Google:ClientSecret"]!;
                options.SignInScheme = ExternalAuthenticationScheme;
                options.CallbackPath = "/api/auth/oauth/google/signin";
                options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
                options.TokenEndpoint = "https://oauth2.googleapis.com/token";
                options.UserInformationEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";
                options.SaveTokens = true;

                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = LoadOAuthUserInfoAsync
                };
            });
        }

        if (OAuthProviderIsConfigured(configuration, "GitHub"))
        {
            authenticationBuilder.AddOAuth(GitHubAuthenticationScheme, options =>
            {
                options.ClientId = configuration["OAuth:GitHub:ClientId"]!;
                options.ClientSecret = configuration["OAuth:GitHub:ClientSecret"]!;
                options.SignInScheme = ExternalAuthenticationScheme;
                options.CallbackPath = "/api/auth/oauth/github/signin";
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";
                options.SaveTokens = true;

                options.Scope.Add("user:email");

                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey("urn:github:login", "login");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = LoadGitHubUserInfoAsync
                };
            });
        }

        authenticationBuilder
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        var userIdValue = principal?.FindFirstValue("userId") ??
                                          principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var sessionIdValue = principal?.FindFirstValue("sessionId");

                        if (!Guid.TryParse(userIdValue, out var userId) ||
                            !Guid.TryParse(sessionIdValue, out var sessionId))
                        {
                            context.Fail("The access token session is invalid.");
                            return;
                        }

                        var db = context.HttpContext.RequestServices
                            .GetRequiredService<ContentHubDbContext>();

                        var now = DateTime.UtcNow;

                        var sessionIsActive = await db.UserSessions
                            .AsNoTracking()
                            .AnyAsync(session =>
                                    session.Id == sessionId &&
                                    session.UserId == userId &&
                                    session.RevokedAtUtc == null &&
                                    session.ExpiresAtUtc > now &&
                                    session.User.Status == UserStatus.Active,
                                context.HttpContext.RequestAborted);

                        if (!sessionIsActive)
                        {
                            context.Fail("The access token session is no longer active.");
                        }
                    },

                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = ApiResponse<object>.Fail(
                            ApiError.Create(
                                code: "auth.unauthorized",
                                message: "Authentication is required."));

                        await context.Response.WriteAsJsonAsync(response);
                    },

                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = ApiResponse<object>.Fail(
                            ApiError.Create(
                                code: "auth.forbidden",
                                message: "You do not have permission to access this resource."));

                        await context.Response.WriteAsJsonAsync(response);
                    }
                };
            });

        return services;
    }

    private static bool OAuthProviderIsConfigured(IConfiguration configuration, string provider)
    {
        return !string.IsNullOrWhiteSpace(configuration[$"OAuth:{provider}:ClientId"]) &&
               !string.IsNullOrWhiteSpace(configuration[$"OAuth:{provider}:ClientSecret"]);
    }

    private static async Task LoadOAuthUserInfoAsync(OAuthCreatingTicketContext context)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            context.Options.UserInformationEndpoint);

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            context.AccessToken);

        using var response = await context.Backchannel.SendAsync(
            request,
            context.HttpContext.RequestAborted);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(
            context.HttpContext.RequestAborted);

        using var payload = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: context.HttpContext.RequestAborted);

        context.RunClaimActions(payload.RootElement);
    }

    private static async Task LoadGitHubUserInfoAsync(OAuthCreatingTicketContext context)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            context.Options.UserInformationEndpoint);

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            context.AccessToken);
        request.Headers.UserAgent.ParseAdd("ContentHub");

        using var response = await context.Backchannel.SendAsync(
            request,
            context.HttpContext.RequestAborted);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(
            context.HttpContext.RequestAborted);

        using var payload = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: context.HttpContext.RequestAborted);

        context.RunClaimActions(payload.RootElement);

        if (!string.IsNullOrWhiteSpace(context.Principal?.FindFirstValue(ClaimTypes.Email)))
        {
            return;
        }

        var email = await GetGitHubPrimaryEmailAsync(context);

        if (!string.IsNullOrWhiteSpace(email))
        {
            context.Identity?.AddClaim(new Claim(ClaimTypes.Email, email));
        }
    }

    private static async Task<string?> GetGitHubPrimaryEmailAsync(OAuthCreatingTicketContext context)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.github.com/user/emails");

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            context.AccessToken);
        request.Headers.UserAgent.ParseAdd("ContentHub");

        using var response = await context.Backchannel.SendAsync(
            request,
            context.HttpContext.RequestAborted);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(
            context.HttpContext.RequestAborted);

        using var payload = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: context.HttpContext.RequestAborted);

        foreach (var emailElement in payload.RootElement.EnumerateArray())
        {
            var isPrimary = emailElement.TryGetProperty("primary", out var primaryElement) &&
                            primaryElement.GetBoolean();
            var isVerified = emailElement.TryGetProperty("verified", out var verifiedElement) &&
                             verifiedElement.GetBoolean();

            if (!isPrimary || !isVerified)
            {
                continue;
            }

            if (emailElement.TryGetProperty("email", out var emailValue))
            {
                return emailValue.GetString();
            }
        }

        return null;
    }
}
