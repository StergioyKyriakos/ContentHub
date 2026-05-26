using ContentHub.Application.Common.Security;

namespace ContentHub.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddContentHubAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AuthenticatedOnly, policy =>
            {
                policy.RequireAuthenticatedUser();
            });

            options.AddPolicy(Policies.AdminOnly, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Admin);
            });

            options.AddPolicy(Policies.EditorOrAdmin, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Editor, Roles.Admin);
            });

            options.AddPolicy(Policies.AuthorOrEditorOrAdmin, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Author, Roles.Editor, Roles.Admin);
            });
        });

        return services;
    }
}