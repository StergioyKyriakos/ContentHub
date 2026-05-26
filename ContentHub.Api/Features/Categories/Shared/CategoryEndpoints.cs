namespace ContentHub.Api.Features.Categories.Shared;

public static class CategoryEndpoints
{
    public const string Create = "/api/categories";

    public const string Update = "/api/categories/{id:guid}";

    public const string Delete = "/api/categories/{id:guid}";

    public const string GetById = "/api/categories/{id:guid}";

    public const string GetAll = "/api/categories";
}