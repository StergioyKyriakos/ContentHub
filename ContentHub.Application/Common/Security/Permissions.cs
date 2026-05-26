namespace ContentHub.Application.Common.Security;

public static class Permissions
{
    public static class Posts
    {
        public const string Create = "posts:create";
        public const string Read = "posts:read";
        public const string Update = "posts:update";
        public const string Delete = "posts:delete";
        public const string Publish = "posts:publish";
        public const string Feature = "posts:feature";
    }

    public static class Categories
    {
        public const string Create = "categories:create";
        public const string Read = "categories:read";
        public const string Update = "categories:update";
        public const string Delete = "categories:delete";
    }

    public static class Authors
    {
        public const string Create = "authors:create";
        public const string Read = "authors:read";
        public const string Update = "authors:update";
        public const string Delete = "authors:delete";
    }

    public static class Assets
    {
        public const string Upload = "assets:upload";
        public const string Read = "assets:read";
        public const string Delete = "assets:delete";
    }

    public static class Users
    {
        public const string Read = "users:read";
        public const string Update = "users:update";
        public const string Disable = "users:disable";
        public const string ManageRoles = "users:manage_roles";
    }

    public static class AuditLogs
    {
        public const string Read = "audit_logs:read";
        public const string Export = "audit_logs:export";
    }

    public static class System
    {
        public const string ReadHealth = "system:read_health";
        public const string ReadInfo = "system:read_info";
        public const string ManageSettings = "system:manage_settings";
    }
}