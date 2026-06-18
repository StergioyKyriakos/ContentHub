using System.Security.Claims;
using System.Text;
using ContentHub.Data.Enums;
using ContentHub.Data.Dtos.Common;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ContentHub.Api.Extensions;

public static class AuthenticationExtensions
{
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

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
}
