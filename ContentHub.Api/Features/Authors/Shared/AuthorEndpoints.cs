namespace ContentHub.Api.Features.Authors.Shared;

public static class AuthorEndpoints
{
    public const string Create = "/api/authors";

    public const string Update = "/api/authors/{id:guid}";

    public const string Delete = "/api/authors/{id:guid}";

    public const string GetById = "/api/authors/{id:guid}";

    public const string GetAll = "/api/authors";

    public const string GetPosts = "/api/authors/{id:guid}/posts";
}