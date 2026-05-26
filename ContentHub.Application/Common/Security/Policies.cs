namespace ContentHub.Application.Common.Security;

public static class Policies
{
    public const string AdminOnly = "AdminOnly";

    public const string EditorOrAdmin = "EditorOrAdmin";

    public const string AuthorOrEditorOrAdmin = "AuthorOrEditorOrAdmin";

    public const string AuthenticatedOnly = "AuthenticatedOnly";
}